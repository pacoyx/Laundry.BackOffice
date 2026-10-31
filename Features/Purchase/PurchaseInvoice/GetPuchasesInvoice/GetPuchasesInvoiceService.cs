
using Microsoft.EntityFrameworkCore;

public class GetPuchasesInvoiceService : IGetPuchasesInvoiceService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<GetServicesAccessFastService> Logger { get; }

    public GetPuchasesInvoiceService(RecepcionDbContext context, IAppLogger<GetServicesAccessFastService> logger)
    {
        _context = context;
        Logger = logger;
    }

    async public Task<GetPurchaseInvoiceByIdResponseDto> GetPurchaseInvoiceAsync(int id)
    {
        var purchaseInvoice = await _context.PurchaseInvoices
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.PurchaseInvoiceDetails!)
                .ThenInclude(x => x.Product)
            .Include(x => x.Payments)
                .ThenInclude(x => x.PaymentMethod)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (purchaseInvoice == null)
        {
            return null; // Or throw an exception if you prefer
        }

        var response = new GetPurchaseInvoiceByIdResponseDto
        {
            Id = purchaseInvoice.Id,
            InvoiceType = purchaseInvoice.InvoiceType,
            InvoiceSerie = purchaseInvoice.InvoiceSerie,
            InvoiceNumber = purchaseInvoice.InvoiceNumber,
            InvoiceIssueDate = purchaseInvoice.InvoiceIssueDate,
            InvoiceExpirationDate = purchaseInvoice.InvoiceExpirationDate,
            TypePayment = purchaseInvoice.TypePayment,
            DaysCredit = purchaseInvoice.DaysCredit,
            PaymentMethod = purchaseInvoice.PaymentMethod,
            Total = purchaseInvoice.Total,
            Status = purchaseInvoice.Status,
            AmountPaid = purchaseInvoice.AmountPaid,
            Balance = purchaseInvoice.Balance,
            PaymentStatus = purchaseInvoice.PaymentStatus,
            PurchaseDate = purchaseInvoice.PurchaseDate,
            SupplierId = purchaseInvoice.SupplierId,
            SupplierName = purchaseInvoice.Supplier?.Name ?? string.Empty,
            SupplierRuc = purchaseInvoice.Supplier?.Ruc ?? string.Empty,
            Subtotal = purchaseInvoice.Subtotal,
            Igv = purchaseInvoice.Igv,
            CurrencyId = purchaseInvoice.CurrencyId,
            ExchangeRate = purchaseInvoice.ExchangeRate,
            Comments = purchaseInvoice.Comments ?? string.Empty,
            PurchaseInvoiceDetails = (purchaseInvoice.PurchaseInvoiceDetails ?? new List<PurchaseInvoiceDetails>()).Select(detail => new GetPurchaseInvoiceByIdDetailsResponseDto
            {
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                Price = detail.Price,
                Total = detail.Total,
                ProductName = detail.Product != null ? detail.Product.Name : string.Empty,
                Comments = detail.Comments ?? string.Empty
            }).ToList(),
            Payments = purchaseInvoice.Payments.Select(payment => new RegisterPurchasePaymentResponseDto(
                payment.Id, payment.PurchaseInvoiceId, payment.PaymentMethodId, payment.PaymentMethod.Name,
                payment.PaymentDate, payment.Amount, payment.OperationNumber, payment.Observation
            )).ToList()
        };

        return response;
    }

    async public Task<IEnumerable<GetPurchaseInvoiceResponseDto>> GetPurchasesInvoiceAsync(
        int? supplierId,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? status,
        string? invoiceNumber = null)
    {
        var query = _context.PurchaseInvoices
            .AsNoTracking()
            .Include(x => x.Supplier)
            .AsQueryable();

        if (supplierId.HasValue)
        {
            query = query.Where(x => x.SupplierId == supplierId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(x => x.InvoiceIssueDate.Date >= dateFrom.Value.Date);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(x => x.InvoiceIssueDate.Date <= dateTo.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(invoiceNumber))
        {
            query = query.Where(x => x.InvoiceNumber.Contains(invoiceNumber));
        }

        var purchaseInvoices = await query
            .OrderByDescending(x => x.InvoiceIssueDate)
            .Select(x => new GetPurchaseInvoiceResponseDto
            {
                Id = x.Id,
                InvoiceType = x.InvoiceType,
                InvoiceSerie = x.InvoiceSerie,
                InvoiceNumber = x.InvoiceNumber,
                InvoiceIssueDate = x.InvoiceIssueDate,
                InvoiceExpirationDate = x.InvoiceExpirationDate,
                TypePayment = x.TypePayment,
                DaysCredit = x.DaysCredit,
                PaymentMethod = x.PaymentMethod,
                Total = x.Total,
                Status = x.Status,
                AmountPaid = x.AmountPaid,
                Balance = x.Balance,
                PaymentStatus = x.PaymentStatus,
                PurchaseDate = x.PurchaseDate,
                SupplierId = x.SupplierId,
                SupplierName = x.Supplier != null ? x.Supplier.Name : string.Empty // Safely accessing Supplier.Name
            }).ToListAsync();

        return purchaseInvoices;
    }
}