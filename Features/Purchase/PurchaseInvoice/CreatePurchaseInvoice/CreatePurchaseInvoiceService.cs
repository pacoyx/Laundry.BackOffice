
using Microsoft.EntityFrameworkCore;
using Serilog;

public class CreatePurchaseInvoiceService : ICreatePurchaseInvoiceService
{
    private readonly RecepcionDbContext _context;
    private readonly IInventoryPostingService _inventoryPosting;
    public IAppLogger<GetServicesAccessFastService> Logger { get; }

    public CreatePurchaseInvoiceService(RecepcionDbContext context, IAppLogger<GetServicesAccessFastService> logger,
        IInventoryPostingService inventoryPosting)
    {
        _context = context;
        Logger = logger;
        _inventoryPosting = inventoryPosting;
    }

    public async Task<PurchaseInvoiceResponseDto> CreatePurchaseInvoiceAsync(PurchaseInvoiceRequestDto request, int? userId = null)
    {
        if (request.WarehouseId.HasValue && !await _context.Warehouses.AnyAsync(w => w.Id == request.WarehouseId.Value && w.IsActive))
        {
            throw new InventoryException("El almacén de destino no existe o está inactivo.");
        }


        // Convert the incoming date to UTC and then to the desired timezone
        // var invoiceIssueDateUnspecified = DateTime.SpecifyKind(request.InvoiceIssueDate, DateTimeKind.Unspecified);
        // var invoiceIssueDateUtc = TimeZoneInfo.ConvertTimeToUtc(invoiceIssueDateUnspecified, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
        var invoiceIssueDateLocal = TimeZoneInfo.ConvertTimeFromUtc(request.InvoiceIssueDate.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
        invoiceIssueDateLocal = invoiceIssueDateLocal.Date; // Set time to 00:00:00

        Logger.LogInformacion("fecha emision 3 " + invoiceIssueDateLocal);  
        
        
        
        DateTime fechaOperacion = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
        var purchaseInvoice = new PurchaseInvoice
        {
            SupplierId = request.SupplierId,
            InvoiceType = request.InvoiceType,
            InvoiceSerie = request.InvoiceSerie,
            InvoiceNumber = request.InvoiceNumber,
            InvoiceIssueDate = request.InvoiceIssueDate,
            InvoiceExpirationDate = request.InvoiceExpirationDate,
            TypePayment = request.TypePayment,
            DaysCredit = request.DaysCredit,
            PaymentMethod = request.PaymentMethod,
            Total = request.Total,
            Status = "A",
            AmountPaid = 0,
            Balance = request.Total,
            PaymentStatus = PurchaseInvoiceStatus.Pending,
            PurchaseDate = fechaOperacion,
            CurrencyId = request.CurrencyId,
            ExchangeRate = request.ExchangeRate,
            Comments = request.Comments,
            Subtotal = request.Subtotal,
            Igv = request.Igv,
            PurchaseInvoiceDetails = request.PurchaseInvoiceDetails?
            .Select(detail => new PurchaseInvoiceDetails
            {
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                Price = detail.Price,
                Total = detail.Total
            }).ToList()
        };

        purchaseInvoice.WarehouseId = request.WarehouseId;

        // La compra y su ingreso al almacén se guardan juntos: si el movimiento falla, no queda la compra.
        await using var transaction = await _context.Database.BeginTransactionAsync();

        await _context.PurchaseInvoices.AddAsync(purchaseInvoice);
        await _context.SaveChangesAsync();

        if (request.WarehouseId.HasValue)
        {
            await PostPurchaseInAsync(purchaseInvoice, request.WarehouseId.Value, userId);
        }

        await transaction.CommitAsync();

        return new PurchaseInvoiceResponseDto
        {
            Id = purchaseInvoice.Id
        };
    }

    /// <summary>
    /// Registra el "Ingreso por compra" de los productos que controlan stock. El costo se lleva sin IGV
    /// (el IGV de compra es crédito fiscal) y en soles: total de la línea × (Subtotal / Total) / cantidad,
    /// multiplicado por el tipo de cambio si la compra es en otra moneda.
    /// </summary>
    private async Task PostPurchaseInAsync(PurchaseInvoice invoice, int warehouseId, int? userId)
    {
        var details = invoice.PurchaseInvoiceDetails ?? new List<PurchaseInvoiceDetails>();
        var productIds = details.Select(d => d.ProductId).Distinct().ToList();
        var tracked = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.TracksStock)
            .Select(p => p.Id)
            .ToListAsync();

        var netRatio = invoice.Total > 0 ? invoice.Subtotal / invoice.Total : 1m;
        var exchangeRate = invoice.CurrencyId == "PEN" || invoice.ExchangeRate <= 0 ? 1m : invoice.ExchangeRate;

        // Una línea por producto (si se repite en la factura se agrupa, con su costo ponderado).
        var lines = details
            .Where(d => tracked.Contains(d.ProductId) && d.Quantity > 0)
            .GroupBy(d => d.ProductId)
            .Select(g =>
            {
                var quantity = g.Sum(d => d.Quantity);
                var netTotal = g.Sum(d => d.Total) * netRatio * exchangeRate;
                return new InventoryDraftLine(g.Key, quantity, Math.Round(netTotal / quantity, 4));
            })
            .ToList();

        if (lines.Count == 0)
        {
            return;
        }

        await _inventoryPosting.PostAsync(new InventoryMovementDraft
        {
            Type = InventoryMovementType.PurchaseIn,
            MovementDate = InventoryPostingService.NowLima(),
            WarehouseId = warehouseId,
            PurchaseInvoiceId = invoice.Id,
            Reason = $"{invoice.InvoiceSerie}-{invoice.InvoiceNumber}",
            CreatedByUserId = userId,
            Lines = lines
        });
    }
}