// ---- Almacenes ----

public record WarehouseDto
{
    public int Id { get; init; }
    public int BranchSalesId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
}

public record SaveWarehouseRequest
{
    public int BranchSalesId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; } = true;
}

// ---- Movimientos ----

public record CreateInventoryMovementLineRequest
{
    public int ProductId { get; init; }
    public decimal Quantity { get; init; }
    /// <summary>Requerido en inventario inicial; opcional en ajuste de ingreso (por defecto, costo promedio).</summary>
    public decimal? UnitCost { get; init; }
}

public record CreateInventoryMovementRequest
{
    public InventoryMovementType Type { get; init; }
    public int WarehouseId { get; init; }
    public int? TargetWarehouseId { get; init; }
    public int? AreaId { get; init; }
    public string? Reason { get; init; }
    public List<CreateInventoryMovementLineRequest> Lines { get; init; } = new();
}

public record CancelInventoryMovementRequest
{
    public string? Reason { get; init; }
}

public record InventoryMovementListItemDto
{
    public long Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public InventoryMovementType Type { get; init; }
    public InventoryMovementStatus Status { get; init; }
    public DateTime MovementDate { get; init; }
    public int WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public string? TargetWarehouseName { get; init; }
    public string? AreaName { get; init; }
    public string? Reason { get; init; }
    public int? PurchaseInvoiceId { get; init; }
    public string? PurchaseInvoiceRef { get; init; }
    public int LineCount { get; init; }
    /// <summary>Valor total del movimiento (suma de líneas del almacén principal, en valor absoluto).</summary>
    public decimal TotalCost { get; init; }
}

public record InventoryMovementLineDto
{
    public long Id { get; init; }
    public int ProductId { get; init; }
    public string? ProductCode { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? UnitName { get; init; }
    public int WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal TotalCost { get; init; }
    public decimal BalanceQuantity { get; init; }
    public decimal BalanceAverageCost { get; init; }
}

public record InventoryMovementDetailDto : InventoryMovementListItemDto
{
    public int? AreaId { get; init; }
    public int? TargetWarehouseId { get; init; }
    public long? ReversedMovementId { get; init; }
    public string? ReversedMovementNumber { get; init; }
    public long? ReversalMovementId { get; init; }
    public string? ReversalMovementNumber { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedByUserName { get; init; }
    public List<InventoryMovementLineDto> Lines { get; init; } = new();
}

// ---- Stock ----

public record StockItemDto
{
    public int ProductId { get; init; }
    public string? ProductCode { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? CategoryName { get; init; }
    public string? UnitName { get; init; }
    /// <summary>Null cuando la consulta agrupa todos los almacenes.</summary>
    public int? WarehouseId { get; init; }
    public string? WarehouseName { get; init; }
    public decimal Quantity { get; init; }
    public decimal AverageCost { get; init; }
    public decimal TotalValue { get; init; }
    public decimal MinStock { get; init; }
    public bool BelowMin { get; init; }
}

public record ProductStockByWarehouseDto
{
    public int WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public string BranchName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal AverageCost { get; init; }
    public decimal TotalValue { get; init; }
}

// ---- Kardex ----

public record KardexLineDto
{
    public long MovementId { get; init; }
    public string MovementNumber { get; init; } = string.Empty;
    public DateTime MovementDate { get; init; }
    public InventoryMovementType Type { get; init; }
    public InventoryMovementStatus Status { get; init; }
    public string? Detail { get; init; }
    public decimal QuantityIn { get; init; }
    public decimal QuantityOut { get; init; }
    public decimal UnitCost { get; init; }
    public decimal TotalCost { get; init; }
    public decimal BalanceQuantity { get; init; }
    public decimal BalanceAverageCost { get; init; }
    public decimal BalanceValue { get; init; }
}

public record KardexDto
{
    public int ProductId { get; init; }
    public string? ProductCode { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? UnitName { get; init; }
    public int WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public decimal OpeningQuantity { get; init; }
    public decimal OpeningAverageCost { get; init; }
    public decimal OpeningValue { get; init; }
    public decimal TotalIn { get; init; }
    public decimal TotalOut { get; init; }
    public decimal ClosingQuantity { get; init; }
    public decimal ClosingAverageCost { get; init; }
    public decimal ClosingValue { get; init; }
    public List<KardexLineDto> Lines { get; init; } = new();
}

public static class InventoryLabels
{
    public static string MovementType(InventoryMovementType type) => type switch
    {
        InventoryMovementType.InitialBalance => "Inventario inicial",
        InventoryMovementType.PurchaseIn => "Ingreso por compra",
        InventoryMovementType.AdjustmentIn => "Ajuste de ingreso",
        InventoryMovementType.AdjustmentOut => "Ajuste de salida",
        InventoryMovementType.Consumption => "Consumo interno",
        InventoryMovementType.Transfer => "Transferencia",
        InventoryMovementType.Reversal => "Anulación",
        InventoryMovementType.SaleOut => "Salida por venta",
        _ => type.ToString()
    };
}
