public interface ICreatePurchaseInvoiceService
{
    Task<PurchaseInvoiceResponseDto> CreatePurchaseInvoiceAsync(PurchaseInvoiceRequestDto request, int? userId = null);
}
