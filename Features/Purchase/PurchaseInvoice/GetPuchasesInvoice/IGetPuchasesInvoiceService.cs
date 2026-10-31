public interface IGetPuchasesInvoiceService
{
    Task<GetPurchaseInvoiceByIdResponseDto> GetPurchaseInvoiceAsync(int id);
    Task<IEnumerable<GetPurchaseInvoiceResponseDto>> GetPurchasesInvoiceAsync(
        int? supplierId,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? status,
        string? invoiceNumber = null);
}

