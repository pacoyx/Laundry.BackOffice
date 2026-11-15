using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public interface ISaleService
{
    Task<PosContextDto> GetPosContextAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<SaleProductDto>> SearchProductsAsync(string? term, int? warehouseId, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(CreateSaleRequest request, int userId, CancellationToken cancellationToken = default);
    Task CancelAsync(long saleId, string reason, int userId, CancellationToken cancellationToken = default);
    Task<SaleDetailDto?> GetAsync(long saleId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Ventas de mostrador: una venta se registra junto con su salida de inventario (SaleOut), su cobro en la
/// caja abierta del usuario y su número correlativo, todo en una sola transacción.
/// </summary>
public class SaleService : ISaleService
{
    private readonly RecepcionDbContext _context;
    private readonly IInventoryPostingService _inventory;
    private readonly ICompanyProfileService _company;
    private readonly decimal _igvRate;

    public SaleService(RecepcionDbContext context, IInventoryPostingService inventory, ICompanyProfileService company,
        IOptions<InvoiceSettings> invoiceSettings)
    {
        _context = context;
        _inventory = inventory;
        _company = company;
        _igvRate = invoiceSettings.Value.IgvRate > 0 ? invoiceSettings.Value.IgvRate : 0.18m;
    }

    private Task<CashBoxMain?> OpenCashBoxAsync(int userId, CancellationToken ct) =>
        _context.CashBoxMains.AsNoTracking()
            .Where(c => c.UserId == userId && c.EstadoRegistro == "A" && c.EstadoCaja == "A")
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(ct);

    private async Task<Customer> WalkInCustomerAsync(CancellationToken ct) =>
        await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.InternalCode == WalkInCustomerSeeder.InternalCode, ct)
        ?? throw new SaleException("No existe el cliente \"CLIENTES VARIOS\". Reinicie la API para que se cree.");

    public async Task<PosContextDto> GetPosContextAsync(int userId, CancellationToken cancellationToken = default)
    {
        var walkIn = await WalkInCustomerAsync(cancellationToken);
        var cashBox = await OpenCashBoxAsync(userId, cancellationToken);
        if (cashBox is null)
        {
            return new PosContextDto
            {
                CashBoxOpen = false,
                WalkInCustomerId = walkIn.Id,
                WalkInCustomerName = walkIn.FirtsName,
                IgvRate = _igvRate
            };
        }

        var branchName = await _context.BranchSales.Where(b => b.Id == cashBox.BranchSalesId).Select(b => b.Description).FirstOrDefaultAsync(cancellationToken);
        var warehouses = await _context.Warehouses.AsNoTracking()
            .Where(w => w.BranchSalesId == cashBox.BranchSalesId && w.IsActive)
            .OrderByDescending(w => w.IsDefault).ThenBy(w => w.Name)
            .Select(w => new PosWarehouseDto(w.Id, w.Name, w.IsDefault))
            .ToListAsync(cancellationToken);

        return new PosContextDto
        {
            CashBoxOpen = true,
            CashBoxId = cashBox.Id,
            BranchSalesId = cashBox.BranchSalesId,
            BranchName = branchName,
            Warehouses = warehouses,
            DefaultWarehouseId = warehouses.FirstOrDefault(w => w.IsDefault)?.Id ?? warehouses.FirstOrDefault()?.Id,
            WalkInCustomerId = walkIn.Id,
            WalkInCustomerName = walkIn.FirtsName,
            IgvRate = _igvRate
        };
    }

    public async Task<List<SaleProductDto>> SearchProductsAsync(string? term, int? warehouseId, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsNoTracking().Where(p => p.Status == "A" && p.IsSellable);
        var t = (term ?? string.Empty).Trim();
        if (t.Length > 0)
        {
            query = query.Where(p => p.Name.Contains(t) || (p.Code != null && p.Code.Contains(t)));
        }

        return await query
            .OrderBy(p => p.Name)
            .Take(30)
            .Select(p => new SaleProductDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                UnitMeasurementDescription = p.UnitMeasurement != null ? p.UnitMeasurement.Abbreviation : string.Empty,
                Price = p.Price,
                TracksStock = p.TracksStock,
                Stock = !p.TracksStock ? 0 : _context.ProductStocks
                    .Where(s => s.ProductId == p.Id && (warehouseId == null || s.WarehouseId == warehouseId) && s.Warehouse!.IsActive)
                    .Sum(s => (decimal?)s.Quantity) ?? 0
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<long> CreateAsync(CreateSaleRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var cashBox = await OpenCashBoxAsync(userId, cancellationToken)
            ?? throw new SaleException("No tiene una caja abierta. Abra la caja antes de vender.");

        // Almacén: el indicado o el por defecto de la sucursal de la caja; siempre de esa sucursal.
        var warehouse = request.WarehouseId.HasValue
            ? await _context.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.Id == request.WarehouseId.Value && w.IsActive, cancellationToken)
            : await _context.Warehouses.AsNoTracking()
                .Where(w => w.BranchSalesId == cashBox.BranchSalesId && w.IsActive)
                .OrderByDescending(w => w.IsDefault).FirstOrDefaultAsync(cancellationToken);
        if (warehouse is null)
            throw new SaleException("No hay un almacén activo para la sucursal de la caja.");
        if (warehouse.BranchSalesId != cashBox.BranchSalesId)
            throw new SaleException("El almacén no pertenece a la sucursal de su caja.");

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _context.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);
        foreach (var line in request.Lines)
        {
            if (!products.TryGetValue(line.ProductId, out var p) || p.Status != "A")
                throw new SaleException($"El producto #{line.ProductId} no existe o está inactivo.");
            if (!p.IsSellable)
                throw new SaleException($"\"{p.Name}\" no está habilitado para la venta.");
        }

        Customer customer;
        if (request.CustomerId.HasValue)
        {
            customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.CustomerId.Value, cancellationToken)
                ?? throw new SaleException("El cliente no existe.");
        }
        else
        {
            customer = await WalkInCustomerAsync(cancellationToken);
        }

        // Totales calculados en el servidor (precios con IGV incluido).
        var total = Math.Round(request.Lines.Sum(l => Math.Round(l.Quantity * l.UnitPrice, 2)), 2);
        var subtotal = Math.Round(total / (1 + _igvRate), 2);
        var igv = total - subtotal;

        decimal amountReceived;
        if (request.PaymentType == SalePaymentTypes.Cash)
        {
            amountReceived = request.AmountReceived <= 0 ? total : request.AmountReceived;
            if (amountReceived < total)
                throw new SaleException($"El monto recibido ({amountReceived:N2}) es menor al total ({total:N2}).");
        }
        else
        {
            amountReceived = total;
        }

        var now = InventoryPostingService.NowLima();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var number = await NextNumberAsync(cashBox.BranchSalesId, cancellationToken);

        var sale = new Sale
        {
            Number = number.Number,
            BranchSalesId = cashBox.BranchSalesId,
            WarehouseId = warehouse.Id,
            CashBoxMainId = cashBox.Id,
            CustomerId = customer.Id,
            CustomerName = $"{customer.FirtsName} {customer.LastName}".Trim(),
            UserId = userId,
            SaleDate = now,
            Total = total,
            Subtotal = subtotal,
            Igv = igv,
            PaymentType = request.PaymentType,
            PaymentDescription = string.IsNullOrWhiteSpace(request.PaymentDescription) ? null : request.PaymentDescription.Trim(),
            AmountReceived = amountReceived,
            Change = amountReceived - total,
            Status = SaleStatus.Completed,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            Details = request.Lines.Select(l => new SaleDetail
            {
                ProductId = l.ProductId,
                Description = products[l.ProductId].Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Total = Math.Round(l.Quantity * l.UnitPrice, 2)
            }).ToList()
        };
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync(cancellationToken);

        // Salida de inventario solo para los productos que controlan stock.
        var stockLines = request.Lines
            .Where(l => products[l.ProductId].TracksStock)
            .GroupBy(l => l.ProductId)
            .Select(g => new InventoryDraftLine(g.Key, g.Sum(l => l.Quantity), null))
            .ToList();
        if (stockLines.Count > 0)
        {
            try
            {
                var movement = await _inventory.PostAsync(new InventoryMovementDraft
                {
                    Type = InventoryMovementType.SaleOut,
                    MovementDate = now,
                    WarehouseId = warehouse.Id,
                    SaleId = sale.Id,
                    Reason = sale.Number,
                    CreatedByUserId = userId,
                    Lines = stockLines
                }, cancellationToken);

                sale.InventoryMovementId = movement.Id;
                var costs = movement.Lines.ToDictionary(l => l.ProductId, l => l.UnitCost);
                foreach (var detail in sale.Details.Where(d => costs.ContainsKey(d.ProductId)))
                {
                    detail.UnitCost = costs[detail.ProductId];
                }
            }
            catch (InventoryException ex)
            {
                throw new SaleException(ex.Message);
            }
        }

        // Cobro en la caja del usuario (mismo formato que los pagos de recepción).
        var cashDetail = new CashBoxDetail
        {
            TipoComprobante = "NV",
            SerieComprobante = number.Serie,
            NumComprobante = number.Correlative,
            FechaComprobante = now,
            Importe = total,
            Adelanto = 0,
            TipoPago = request.PaymentType,
            DescripcionPago = sale.PaymentDescription ?? SalePaymentTypes.Label(request.PaymentType),
            Observaciones = $"Venta {sale.Number}",
            EstadoRegistro = "A",
            CustomerId = customer.Id,
            CashBoxMainId = cashBox.Id
        };
        _context.CashBoxDetails.Add(cashDetail);
        await _context.SaveChangesAsync(cancellationToken);

        sale.CashBoxDetailId = cashDetail.Id;
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return sale.Id;
    }

    public async Task CancelAsync(long saleId, string reason, int userId, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == saleId, cancellationToken)
            ?? throw new SaleException("Venta no encontrada.");
        if (sale.Status == SaleStatus.Cancelled)
            throw new SaleException("La venta ya está anulada.");

        // Si la caja ya se cerró, su cierre ya reportó este ingreso: no se puede anular desde aquí.
        var cashBoxOpen = await _context.CashBoxMains.AnyAsync(c => c.Id == sale.CashBoxMainId && c.EstadoCaja == "A", cancellationToken);
        if (!cashBoxOpen)
            throw new SaleException("No se puede anular: la caja de esta venta ya fue cerrada.");

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        if (sale.InventoryMovementId.HasValue)
        {
            try
            {
                await _inventory.ReverseAsync(sale.InventoryMovementId.Value, $"Anulación de la venta {sale.Number}", userId, cancellationToken);
            }
            catch (InventoryException ex)
            {
                throw new SaleException($"No se puede devolver el stock: {ex.Message}");
            }
        }

        if (sale.CashBoxDetailId.HasValue)
        {
            var cashDetail = await _context.CashBoxDetails.FirstOrDefaultAsync(d => d.Id == sale.CashBoxDetailId.Value, cancellationToken);
            if (cashDetail is not null)
            {
                _context.CashBoxDetails.Remove(cashDetail);
            }
        }

        sale.Status = SaleStatus.Cancelled;
        sale.CancelledAt = InventoryPostingService.NowLima();
        sale.CancelledByUserId = userId;
        sale.CancelReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<SaleDetailDto?> GetAsync(long saleId, CancellationToken cancellationToken = default)
    {
        var s = await _context.Sales.AsNoTracking()
            .Include(x => x.BranchSales)
            .Include(x => x.Warehouse)
            .Include(x => x.Customer)
            .Include(x => x.Details).ThenInclude(d => d.Product!).ThenInclude(p => p.UnitMeasurement)
            .FirstOrDefaultAsync(x => x.Id == saleId, cancellationToken);
        if (s is null) return null;

        var userIds = new[] { s.UserId, s.CancelledByUserId ?? 0 };
        var users = await _context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName, cancellationToken);
        var cashBoxOpen = await _context.CashBoxMains.AnyAsync(c => c.Id == s.CashBoxMainId && c.EstadoCaja == "A", cancellationToken);
        var movementNumber = s.InventoryMovementId.HasValue
            ? await _context.InventoryMovements.Where(m => m.Id == s.InventoryMovementId.Value).Select(m => m.Number).FirstOrDefaultAsync(cancellationToken)
            : null;
        var company = await _company.GetCompanyAsync(cancellationToken);

        return new SaleDetailDto
        {
            Id = s.Id,
            Number = s.Number,
            SaleDate = s.SaleDate,
            CustomerId = s.CustomerId,
            CustomerName = s.CustomerName,
            CustomerDocument = s.Customer?.DocPersonal,
            BranchName = s.BranchSales?.Description ?? string.Empty,
            UserName = users.TryGetValue(s.UserId, out var un) ? un : string.Empty,
            PaymentType = s.PaymentType,
            PaymentDescription = s.PaymentDescription,
            Total = s.Total,
            Subtotal = s.Subtotal,
            Igv = s.Igv,
            AmountReceived = s.AmountReceived,
            Change = s.Change,
            Status = s.Status,
            ItemCount = s.Details.Count,
            WarehouseId = s.WarehouseId,
            WarehouseName = s.Warehouse?.Name ?? string.Empty,
            CashBoxMainId = s.CashBoxMainId,
            CashBoxOpen = cashBoxOpen,
            Notes = s.Notes,
            InventoryMovementId = s.InventoryMovementId,
            InventoryMovementNumber = movementNumber,
            CancelledAt = s.CancelledAt,
            CancelledByUserName = s.CancelledByUserId.HasValue && users.TryGetValue(s.CancelledByUserId.Value, out var cn) ? cn : null,
            CancelReason = s.CancelReason,
            TotalCost = Math.Round(s.Details.Sum(d => d.Quantity * d.UnitCost), 2),
            CompanyName = company is null ? string.Empty : (string.IsNullOrWhiteSpace(company.NameComercial) ? company.NameCompany : company.NameComercial),
            CompanyDocument = company is null ? null : $"{company.DocumentType} {company.NumberType}".Trim(),
            CompanyAddress = company?.Address,
            CompanyPhone = company?.Phone,
            Lines = s.Details.OrderBy(d => d.Id).Select(d => new SaleLineDto
            {
                ProductId = d.ProductId,
                ProductCode = d.Product?.Code,
                Description = d.Description,
                UnitName = d.Product?.UnitMeasurement?.Abbreviation,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Total = d.Total,
                UnitCost = d.UnitCost
            }).ToList()
        };
    }

    /// <summary>
    /// Siguiente número de nota de venta de la sucursal. UPDLOCK/HOLDLOCK serializa las ventas simultáneas
    /// sobre el mismo correlativo hasta que termina la transacción (evita números duplicados).
    /// </summary>
    private async Task<(string Number, string Serie, string Correlative)> NextNumberAsync(int branchId, CancellationToken ct)
    {
        var numbering = await _context.NumbersDocuments
            .FromSqlInterpolated($"SELECT * FROM NumbersDocuments WITH (UPDLOCK, HOLDLOCK) WHERE BranchId = {branchId} AND TypeProcess = {SalesNumbersDocumentSeeder.TypeProcess}")
            .FirstOrDefaultAsync(ct)
            ?? throw new SaleException("La sucursal no tiene correlativo de notas de venta. Reinicie la API para que se cree.");

        var correlative = numbering.NumberDoc.ToString("D8");
        numbering.NumberDoc += 1;
        await _context.SaveChangesAsync(ct);
        return ($"{numbering.SerieDoc}-{correlative}", numbering.SerieDoc, correlative);
    }
}
