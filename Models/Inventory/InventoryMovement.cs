public enum InventoryMovementType
{
    /// <summary>Carga inicial de existencias de un almacén.</summary>
    InitialBalance = 1,
    /// <summary>Ingreso automático al registrar una compra con almacén.</summary>
    PurchaseIn = 2,
    AdjustmentIn = 3,
    AdjustmentOut = 4,
    /// <summary>Salida por consumo interno (insumos usados), asignada a un Área.</summary>
    Consumption = 5,
    /// <summary>Transferencia entre almacenes: salida en origen e ingreso en destino.</summary>
    Transfer = 6,
    /// <summary>Movimiento inverso que anula otro movimiento.</summary>
    Reversal = 7,
    /// <summary>Reservado para el módulo de Ventas (fase 2).</summary>
    SaleOut = 8
}

public enum InventoryMovementStatus
{
    Posted = 1,
    Cancelled = 2
}

/// <summary>
/// Documento de movimiento de inventario. Nunca se borra: se anula con un movimiento Reversal.
/// </summary>
public class InventoryMovement
{
    public long Id { get; set; }

    /// <summary>Número visible, derivado del Id (MOV-00000001).</summary>
    public string Number { get; set; } = string.Empty;

    public InventoryMovementType Type { get; set; }

    public InventoryMovementStatus Status { get; set; } = InventoryMovementStatus.Posted;

    public DateTime MovementDate { get; set; }

    public int WarehouseId { get; set; }

    /// <summary>Almacén destino (solo transferencias).</summary>
    public int? TargetWarehouseId { get; set; }

    /// <summary>Área que consume (solo consumo interno).</summary>
    public int? AreaId { get; set; }

    public string? Reason { get; set; }

    /// <summary>Compra que originó el movimiento (PurchaseIn).</summary>
    public int? PurchaseInvoiceId { get; set; }

    /// <summary>En un Reversal: el movimiento que anula.</summary>
    public long? ReversedMovementId { get; set; }

    /// <summary>En un movimiento anulado: el Reversal que lo anuló.</summary>
    public long? ReversalMovementId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public Warehouse? Warehouse { get; set; }
    public Warehouse? TargetWarehouse { get; set; }
    public Area? Area { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }

    public List<InventoryMovementLine> Lines { get; set; } = new();
}

/// <summary>
/// Línea de movimiento. Quantity lleva signo (+ ingresa, − sale) y guarda el saldo resultante para
/// construir el kardex sin recalcular.
/// </summary>
public class InventoryMovementLine
{
    public long Id { get; set; }

    public long InventoryMovementId { get; set; }

    public int ProductId { get; set; }

    /// <summary>Almacén afectado por la línea (en transferencias hay líneas en origen y en destino).</summary>
    public int WarehouseId { get; set; }

    public decimal Quantity { get; set; }

    /// <summary>Costo unitario (sin IGV, en soles) con el que entra o sale.</summary>
    public decimal UnitCost { get; set; }

    public decimal TotalCost { get; set; }

    public decimal BalanceQuantity { get; set; }

    public decimal BalanceAverageCost { get; set; }

    public InventoryMovement? InventoryMovement { get; set; }
    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}
