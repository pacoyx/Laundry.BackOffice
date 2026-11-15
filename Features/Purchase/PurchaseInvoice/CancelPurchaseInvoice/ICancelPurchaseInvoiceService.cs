public interface ICancelPurchaseInvoiceService
{
    Task<CancelPurchaseInvoiceResult> CancelPurchaseInvoiceAsync(int purchaseInvoiceId, int? userId = null);
}
