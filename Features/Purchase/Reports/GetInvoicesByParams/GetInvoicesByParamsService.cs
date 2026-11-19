
using Microsoft.EntityFrameworkCore;

public class GetInvoicesByParamsService : IGetInvoicesByParamsService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<GetInvoicesByParamsService> Logger { get; }

    public GetInvoicesByParamsService(RecepcionDbContext context, IAppLogger<GetInvoicesByParamsService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public async Task<InvoiceListPaginatorResponseDto> GetInvoicesBySupplier(int pageNumber, int pageSize, int supplierId)
    {
        var query = _context.PurchaseInvoices
            .AsNoTracking()
            .Include(i => i.Supplier)
            .Include(i => i.PurchaseInvoiceDetails!)
                .ThenInclude(d => d.Product)
            .Where(i => i.SupplierId == supplierId);

        var totalRows = await query.CountAsync();

        var invoices = await query
            .OrderByDescending(i => i.InvoiceIssueDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (invoices == null || !invoices.Any())
        {
            Logger.LogWarning($"No invoices found for supplier with id {supplierId}.");
            return new InvoiceListPaginatorResponseDto
            {
                Invoices = new List<InvoiceDto>(),
                TotalCount = 0
            };
        }

        var invoiceDtos = new InvoiceListPaginatorResponseDto
        {
            Invoices = invoices.Select(invoice => new InvoiceDto
            {
                Id = invoice.Id,
                SupplierId = invoice.SupplierId,
                SupplierName = invoice.Supplier!.Name,
                SupplierRuc = invoice.Supplier.Ruc,
                InvoiceType = invoice.InvoiceType,
                InvoiceSerie = invoice.InvoiceSerie,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceIssueDate = invoice.InvoiceIssueDate,
                InvoiceExpirationDate = invoice.InvoiceExpirationDate,
                TypePayment = invoice.TypePayment,
                DaysCredit = invoice.DaysCredit,
                PaymentMethod = invoice.PaymentMethod,
                Subtotal = invoice.Subtotal,
                Igv = invoice.Igv,
                Total = invoice.Total,
                Status = invoice.Status,
                CurrencyId = invoice.CurrencyId,
                ExchangeRate = invoice.ExchangeRate,
                Comments = invoice.Comments,
                Details = invoice.PurchaseInvoiceDetails!.Select(detail => new InvoiceDetailsDto
                {
                    ProductId = detail.ProductId,
                    ProductName = detail.Product!.Name,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    Total = detail.Total,
                    Comments = detail.Comments
                }).ToList()
            }).ToList(),
            TotalCount = totalRows
        };

        return invoiceDtos;
    }

    public async Task<InvoiceListPaginatorResponseDto> GetInvoicesByProduct(int pageNumber, int pageSize, string productNamePatron)
    {
         var query = _context.PurchaseInvoices
            .AsNoTracking()
            .Include(i => i.Supplier)
            .Include(i => i.PurchaseInvoiceDetails!)
                .ThenInclude(d => d.Product)
            .Where(i => i.PurchaseInvoiceDetails!.Any(d => d.Product!.Name.Contains(productNamePatron)));

        var totalRows = await query.CountAsync();

        var invoices = await query
            .OrderByDescending(i => i.InvoiceIssueDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (invoices == null || !invoices.Any())
        {
            Logger.LogWarning($"No invoices found for product name pattern '{productNamePatron}'.");
            return new InvoiceListPaginatorResponseDto
            {
                Invoices = new List<InvoiceDto>(),
                TotalCount = 0
            };
        }

        var invoiceDtos = new InvoiceListPaginatorResponseDto
        {
            Invoices = invoices.Select(invoice => new InvoiceDto
            {
                Id = invoice.Id,
                SupplierId = invoice.SupplierId,
                SupplierName = invoice.Supplier!.Name,
                SupplierRuc = invoice.Supplier.Ruc,
                InvoiceType = invoice.InvoiceType,
                InvoiceSerie = invoice.InvoiceSerie,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceIssueDate = invoice.InvoiceIssueDate,
                InvoiceExpirationDate = invoice.InvoiceExpirationDate,
                TypePayment = invoice.TypePayment,
                DaysCredit = invoice.DaysCredit,
                PaymentMethod = invoice.PaymentMethod,
                Subtotal = invoice.Subtotal,
                Igv = invoice.Igv,
                Total = invoice.Total,
                Status = invoice.Status,
                CurrencyId = invoice.CurrencyId,
                ExchangeRate = invoice.ExchangeRate,
                Comments = invoice.Comments,
                Details = invoice.PurchaseInvoiceDetails!.Select(detail => new InvoiceDetailsDto
                {
                    ProductId = detail.ProductId,
                    ProductName = detail.Product!.Name,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    Total = detail.Total,
                    Comments = detail.Comments
                }).ToList()
            }).ToList(),
            TotalCount = totalRows
        };

        return invoiceDtos;
    }

    public async Task<InvoiceListPaginatorResponseDto> GetInvoicesByMonth(int pageNumber, int pageSize, int month, int year)
    {
        var query = _context.PurchaseInvoices
            .AsNoTracking()
            .Include(i => i.Supplier)
            .Include(i => i.PurchaseInvoiceDetails!)
                .ThenInclude(d => d.Product)
            .Where(i => i.InvoiceIssueDate.Month == month && i.InvoiceIssueDate.Year == year);

        var totalRows = await query.CountAsync();

        var invoices = await query
            .OrderByDescending(i => i.InvoiceIssueDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (invoices == null || !invoices.Any())
        {
            Logger.LogWarning($"No invoices found for month {month} and year {year}.");
            return new InvoiceListPaginatorResponseDto
            {
                Invoices = new List<InvoiceDto>(),
                TotalCount = 0
            };
        }

        var invoiceDtos = new InvoiceListPaginatorResponseDto
        {
            Invoices = invoices.Select(invoice => new InvoiceDto
            {
                Id = invoice.Id,
                SupplierId = invoice.SupplierId,
                SupplierName = invoice.Supplier!.Name,
                SupplierRuc = invoice.Supplier.Ruc,
                InvoiceType = invoice.InvoiceType,
                InvoiceSerie = invoice.InvoiceSerie,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceIssueDate = invoice.InvoiceIssueDate,
                InvoiceExpirationDate = invoice.InvoiceExpirationDate,
                TypePayment = invoice.TypePayment,
                DaysCredit = invoice.DaysCredit,
                PaymentMethod = invoice.PaymentMethod,
                Subtotal = invoice.Subtotal,
                Igv = invoice.Igv,
                Total = invoice.Total,
                Status = invoice.Status,
                CurrencyId = invoice.CurrencyId,
                ExchangeRate = invoice.ExchangeRate,
                Comments = invoice.Comments,
                Details = invoice.PurchaseInvoiceDetails!.Select(detail => new InvoiceDetailsDto
                {
                    ProductId = detail.ProductId,
                    ProductName = detail.Product!.Name,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    Total = detail.Total,
                    Comments = detail.Comments
                }).ToList()
            }).ToList(),
            TotalCount = totalRows
        };

        return invoiceDtos;
    }

    public async Task<InvoiceListPaginatorResponseDto> GetInvoicesByDate(int pageNumber, int pageSize, DateTime startDate, DateTime endDate)
    {
        var query = _context.PurchaseInvoices
            .AsNoTracking()
            .Include(i => i.Supplier)
            .Include(i => i.PurchaseInvoiceDetails!)
                .ThenInclude(d => d.Product)
            .Where(i => i.InvoiceIssueDate >= startDate && i.InvoiceIssueDate <= endDate);

        var totalRows = await query.CountAsync();

        var invoices = await query
            .OrderByDescending(i => i.InvoiceIssueDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (invoices == null || !invoices.Any())
        {
            Logger.LogWarning($"No invoices found between {startDate.ToShortDateString()} and {endDate.ToShortDateString()}.");
            return new InvoiceListPaginatorResponseDto
            {
                Invoices = new List<InvoiceDto>(),
                TotalCount = 0
            };
        }

        var invoiceDtos = new InvoiceListPaginatorResponseDto
        {
            Invoices = invoices.Select(invoice => new InvoiceDto
            {
                Id = invoice.Id,
                SupplierId = invoice.SupplierId,
                SupplierName = invoice.Supplier!.Name,
                SupplierRuc = invoice.Supplier.Ruc,
                InvoiceType = invoice.InvoiceType,
                InvoiceSerie = invoice.InvoiceSerie,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceIssueDate = invoice.InvoiceIssueDate,
                InvoiceExpirationDate = invoice.InvoiceExpirationDate,
                TypePayment = invoice.TypePayment,
                DaysCredit = invoice.DaysCredit,
                PaymentMethod = invoice.PaymentMethod,
                Subtotal = invoice.Subtotal,
                Igv = invoice.Igv,
                Total = invoice.Total,
                Status = invoice.Status,
                CurrencyId = invoice.CurrencyId,
                ExchangeRate = invoice.ExchangeRate,
                Comments = invoice.Comments,
                Details = invoice.PurchaseInvoiceDetails!.Select(detail => new InvoiceDetailsDto
                {
                    ProductId = detail.ProductId,
                    ProductName = detail.Product!.Name,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    Total = detail.Total,
                    Comments = detail.Comments
                }).ToList()
            }).ToList(),
            TotalCount = totalRows
        };

        return invoiceDtos;
    }
}