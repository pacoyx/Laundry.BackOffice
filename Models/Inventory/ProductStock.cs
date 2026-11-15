/// <summary>
/// Saldo de un producto en un almacén. Lo actualiza únicamente IInventoryPostingService, en la misma
/// transacción que el movimiento; nunca se edita directamente.
/// </summary>
public class ProductStock
{
    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public decimal Quantity { get; set; }

    /// <summary>Costo promedio ponderado unitario (sin IGV, en soles).</summary>
    public decimal AverageCost { get; set; }

    public DateTime? LastMovementAt { get; set; }

    /// <summary>Token de concurrencia: evita que dos movimientos simultáneos pisen el mismo saldo.</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Product? Product { get; set; }

    public Warehouse? Warehouse { get; set; }
}
