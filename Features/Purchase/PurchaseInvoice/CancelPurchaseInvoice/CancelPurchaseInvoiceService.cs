
using Microsoft.EntityFrameworkCore;

public class CancelPurchaseInvoiceService : ICancelPurchaseInvoiceService
{
    private readonly RecepcionDbContext _context;
    private readonly IInventoryPostingService _inventoryPosting;

    public CancelPurchaseInvoiceService(RecepcionDbContext context, IInventoryPostingService inventoryPosting)
    {
        _context = context;
        _inventoryPosting = inventoryPosting;
    }

    public async Task<CancelPurchaseInvoiceResult> CancelPurchaseInvoiceAsync(int purchaseInvoiceId, int? userId = null)
    {
        var purchaseInvoice = await _context.PurchaseInvoices.FirstOrDefaultAsync(x => x.Id == purchaseInvoiceId);
        if (purchaseInvoice == null)
        {
            return new CancelPurchaseInvoiceResult { NotFound = true };
        }

        if (purchaseInvoice.PaymentStatus == PurchaseInvoiceStatus.Cancelled)
        {
            return new CancelPurchaseInvoiceResult { Error = "La compra ya se encuentra anulada." };
        }

        if (purchaseInvoice.AmountPaid > 0)
        {
            return new CancelPurchaseInvoiceResult { Error = "No se puede anular una compra que ya tiene pagos registrados." };
        }

        // Anular la compra y revertir su ingreso al almacén van juntos: si no se puede revertir (porque los
        // productos ya se consumieron o transfirieron), la compra no se anula.
        await using var transaction = await _context.Database.BeginTransactionAsync();

        purchaseInvoice.PaymentStatus = PurchaseInvoiceStatus.Cancelled;
        await _context.SaveChangesAsync();

        var inventoryMovementId = await _context.InventoryMovements
            .Where(m => m.PurchaseInvoiceId == purchaseInvoiceId
                && m.Type == InventoryMovementType.PurchaseIn
                && m.Status == InventoryMovementStatus.Posted)
            .Select(m => (long?)m.Id)
            .FirstOrDefaultAsync();

        if (inventoryMovementId.HasValue)
        {
            try
            {
                await _inventoryPosting.ReverseAsync(inventoryMovementId.Value,
                    $"Anulación de la compra {purchaseInvoice.InvoiceSerie}-{purchaseInvoice.InvoiceNumber}", userId);
            }
            catch (InventoryException ex)
            {
                await transaction.RollbackAsync();
                return new CancelPurchaseInvoiceResult
                {
                    Error = $"No se puede anular la compra porque su ingreso al almacén ya no se puede revertir. {ex.Message}"
                };
            }
        }

        await transaction.CommitAsync();

        return new CancelPurchaseInvoiceResult { PaymentStatus = purchaseInvoice.PaymentStatus };
    }
}
