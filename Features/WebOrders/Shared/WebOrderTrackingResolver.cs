using Microsoft.EntityFrameworkCore;

public static class WebOrderTrackingResolver
{
    // Exige que orderNumber Y phone coincidan (nunca solo uno) para no exponer el estado de
    // un pedido a cualquiera que adivine/enumere numeros de pedido. Reusado por el endpoint
    // publico /api/web-orders/track y por la tool get_order_status del chatbot.
    public static Task<WebOrder?> FindAsync(RecepcionDbContext db, string orderNumber, string phone, CancellationToken cancellationToken = default)
    {
        return db.WebOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber.Trim() && x.ContactPhone == phone.Trim(), cancellationToken);
    }
}
