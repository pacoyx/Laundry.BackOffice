public enum SaleStatus
{
    Completed = 1,
    Cancelled = 2
}

/// <summary>
/// Venta de mostrador (nota de venta interna). Precios con IGV incluido; el desglose Subtotal/Igv se
/// calcula con InvoiceSettings.IgvRate. Se cobra al contado con un solo medio en la caja abierta del
/// usuario y descuenta inventario con un movimiento SaleOut.
/// </summary>
public class Sale
{
    public long Id { get; set; }

    /// <summary>Número de la nota de venta: serie-correlativo (NV01-00000123).</summary>
    public string Number { get; set; } = string.Empty;

    public int BranchSalesId { get; set; }

    public int WarehouseId { get; set; }

    public int CashBoxMainId { get; set; }

    public int CustomerId { get; set; }

    /// <summary>Nombre del cliente al momento de la venta (copia para el ticket y reportes).</summary>
    public string CustomerName { get; set; } = string.Empty;

    public int UserId { get; set; }

    public DateTime SaleDate { get; set; }

    /// <summary>Total cobrado, con IGV.</summary>
    public decimal Total { get; set; }

    /// <summary>Total sin IGV.</summary>
    public decimal Subtotal { get; set; }

    public decimal Igv { get; set; }

    /// <summary>Código de medio de pago, igual que en caja: EF (efectivo), TA (tarjeta), QR (Yape/Plin).</summary>
    public string PaymentType { get; set; } = string.Empty;

    public string? PaymentDescription { get; set; }

    /// <summary>Monto entregado por el cliente (efectivo) para calcular el vuelto.</summary>
    public decimal AmountReceived { get; set; }

    public decimal Change { get; set; }

    public SaleStatus Status { get; set; } = SaleStatus.Completed;

    public string? Notes { get; set; }

    public DateTime? CancelledAt { get; set; }

    public int? CancelledByUserId { get; set; }

    public string? CancelReason { get; set; }

    public long? InventoryMovementId { get; set; }

    public int? CashBoxDetailId { get; set; }

    public DateTime CreatedAt { get; set; }

    public BranchSales? BranchSales { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Customer? Customer { get; set; }

    public List<SaleDetail> Details { get; set; } = new();
}

public class SaleDetail
{
    public long Id { get; set; }

    public long SaleId { get; set; }

    public int ProductId { get; set; }

    /// <summary>Descripción del producto al momento de la venta.</summary>
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    /// <summary>Precio unitario con IGV.</summary>
    public decimal UnitPrice { get; set; }

    public decimal Total { get; set; }

    /// <summary>Costo promedio unitario con que salió del inventario (0 si el producto no controla stock).</summary>
    public decimal UnitCost { get; set; }

    public Sale? Sale { get; set; }
    public Product? Product { get; set; }
}
