using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public static class ChatbotToolExecutor
{
    public static async Task<string> ExecuteAsync(string toolName, string argumentsJson, RecepcionDbContext db, CancellationToken cancellationToken)
    {
        using var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);

        return toolName switch
        {
            "list_districts" => await ListDistrictsAsync(db, cancellationToken),
            "get_price_list" => await GetPriceListAsync(args.RootElement, db, cancellationToken),
            "get_order_status" => await GetOrderStatusAsync(args.RootElement, db, cancellationToken),
            _ => JsonSerializer.Serialize(new { error = $"Herramienta desconocida: {toolName}" })
        };
    }

    private static async Task<string> ListDistrictsAsync(RecepcionDbContext db, CancellationToken cancellationToken)
    {
        var districts = await db.Districts
            .AsNoTracking()
            .Include(x => x.Zone)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, zone = x.Zone.Name })
            .ToListAsync(cancellationToken);

        return JsonSerializer.Serialize(new { districts });
    }

    private static async Task<string> GetPriceListAsync(JsonElement args, RecepcionDbContext db, CancellationToken cancellationToken)
    {
        if (!args.TryGetProperty("districtId", out var districtIdEl) || !districtIdEl.TryGetInt32(out var districtId))
        {
            return JsonSerializer.Serialize(new { error = "Falta districtId." });
        }

        var (district, priceList) = await ZonePricingResolver.GetActivePriceListForDistrictAsync(db, districtId);

        if (district is null)
        {
            return JsonSerializer.Serialize(new { error = "El distrito indicado no existe o no tiene cobertura." });
        }

        if (priceList is null)
        {
            return JsonSerializer.Serialize(new { error = "No hay una tarifa activa para esa zona por ahora." });
        }

        var items = priceList.Details.Select(d => new
        {
            service = d.ServiceItem.Name,
            unitPrice = d.UnitPrice,
            estimatedTurnaroundHours = d.EstimatedTurnaroundHours
        });

        return JsonSerializer.Serialize(new { zone = district.Zone.Name, district = district.Name, items });
    }

    private static async Task<string> GetOrderStatusAsync(JsonElement args, RecepcionDbContext db, CancellationToken cancellationToken)
    {
        var orderNumber = args.TryGetProperty("orderNumber", out var orderNumberEl) ? orderNumberEl.GetString() : null;
        var phone = args.TryGetProperty("phone", out var phoneEl) ? phoneEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(phone))
        {
            return JsonSerializer.Serialize(new { error = "Falta orderNumber o phone." });
        }

        var webOrder = await WebOrderTrackingResolver.FindAsync(db, orderNumber, phone, cancellationToken);

        if (webOrder is null)
        {
            return JsonSerializer.Serialize(new { error = "No se encontró un pedido con ese número y teléfono. Verifica ambos datos." });
        }

        return JsonSerializer.Serialize(new
        {
            orderNumber = webOrder.OrderNumber,
            status = webOrder.Status.ToString(),
            scheduledPickupDate = webOrder.ScheduledPickupDate,
            createdAt = webOrder.CreatedAt
        });
    }
}
