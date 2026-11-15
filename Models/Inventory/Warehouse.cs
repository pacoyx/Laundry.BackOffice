/// <summary>
/// Almacén físico donde se guardan existencias. Pertenece a una sucursal; cada sucursal tiene un
/// almacén por defecto ("Almacén principal") y puede tener más.
/// </summary>
public class Warehouse
{
    public int Id { get; set; }

    public int BranchSalesId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Almacén por defecto de la sucursal (uno solo por sucursal).</summary>
    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public BranchSales? BranchSales { get; set; }
}
