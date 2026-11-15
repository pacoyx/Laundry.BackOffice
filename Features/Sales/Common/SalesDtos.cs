/// <summary>Error de negocio de ventas (sin caja, producto no vendible, pago insuficiente). Se devuelve como 400.</summary>
public class SaleException : Exception
{
    public SaleException(string message) : base(message) { }
}

public static class SalePaymentTypes
{
    // Mismos códigos que usa la caja (el cierre suma EF, TA y QR).
    public const string Cash = "EF";
    public const string Card = "TA";
    public const string Qr = "QR";

    public static readonly string[] All = { Cash, Card, Qr };

    public static string Label(string code) => code switch
    {
        Cash => "Efectivo",
        Card => "Tarjeta",
        Qr => "Yape / Plin",
        _ => code
    };
}

// ---- Punto de venta ----

public record PosWarehouseDto(int Id, string Name, bool IsDefault);

public record PosContextDto
{
    public bool CashBoxOpen { get; init; }
    public int? CashBoxId { get; init; }
    public int? BranchSalesId { get; init; }
    public string? BranchName { get; init; }
    public List<PosWarehouseDto> Warehouses { get; init; } = new();
    public int? DefaultWarehouseId { get; init; }
    public int WalkInCustomerId { get; init; }
    public string WalkInCustomerName { get; init; } = string.Empty;
    public decimal IgvRate { get; init; }
}

public record SaleProductDto
{
    public int Id { get; init; }
    public string? Code { get; init; }
    public string Name { get; init; } = string.Empty;
    public string UnitMeasurementDescription { get; init; } = string.Empty;
    /// <summary>Precio de venta con IGV.</summary>
    public decimal Price { get; init; }
    public bool TracksStock { get; init; }
    /// <summary>Stock en el almacén consultado (0 si no controla stock).</summary>
    public decimal Stock { get; init; }
}

public record CreateSaleLineRequest
{
    public int ProductId { get; init; }
    public decimal Quantity { get; init; }
    /// <summary>Precio unitario con IGV (puede ajustarse en el punto de venta).</summary>
    public decimal UnitPrice { get; init; }
}

public record CreateSaleRequest
{
    public int? WarehouseId { get; init; }
    /// <summary>Si no se informa, se usa "CLIENTES VARIOS".</summary>
    public int? CustomerId { get; init; }
    public string PaymentType { get; init; } = string.Empty;
    public string? PaymentDescription { get; init; }
    /// <summary>Monto entregado (efectivo). En tarjeta/QR se toma el total.</summary>
    public decimal AmountReceived { get; init; }
    public string? Notes { get; init; }
    public List<CreateSaleLineRequest> Lines { get; init; } = new();
}

public record CancelSaleRequest
{
    public string? Reason { get; init; }
}

// ---- Consulta ----

public record SaleListItemDto
{
    public long Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public DateTime SaleDate { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string BranchName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string PaymentType { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public SaleStatus Status { get; init; }
    public int ItemCount { get; init; }
}

public record SaleLineDto
{
    public int ProductId { get; init; }
    public string? ProductCode { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? UnitName { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Total { get; init; }
    public decimal UnitCost { get; init; }
}

public record SaleDetailDto : SaleListItemDto
{
    public int CustomerId { get; init; }
    public string? CustomerDocument { get; init; }
    public int WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public int CashBoxMainId { get; init; }
    public bool CashBoxOpen { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Igv { get; init; }
    public string? PaymentDescription { get; init; }
    public decimal AmountReceived { get; init; }
    public decimal Change { get; init; }
    public string? Notes { get; init; }
    public long? InventoryMovementId { get; init; }
    public string? InventoryMovementNumber { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CancelledByUserName { get; init; }
    public string? CancelReason { get; init; }
    /// <summary>Costo total de lo vendido (para margen).</summary>
    public decimal TotalCost { get; init; }
    // Datos de la empresa para el ticket
    public string CompanyName { get; init; } = string.Empty;
    public string? CompanyDocument { get; init; }
    public string? CompanyAddress { get; init; }
    public string? CompanyPhone { get; init; }
    public List<SaleLineDto> Lines { get; init; } = new();
}
