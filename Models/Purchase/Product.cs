using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    /// <summary>
    /// Obsoleto: antes se escribía a mano. El stock real sale de ProductStock (movimientos de inventario);
    /// la columna se conserva solo para no perder los datos históricos.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public int Stock { get; set; }
    public int UnitMeasurementId { get; set; }
    public int CategoryProdId { get; set; }
    [MaxLength(1)]
    public string Status { get; set; } = "A";

    /// <summary>Código interno o SKU (opcional, único si se informa).</summary>
    [MaxLength(30)]
    public string? Code { get; set; }

    /// <summary>Si es false (p. ej. un servicio comprado), no genera movimientos de inventario.</summary>
    public bool TracksStock { get; set; } = true;

    /// <summary>Stock mínimo para el aviso de reposición.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal MinStock { get; set; }

    /// <summary>Se puede vender (reservado para el módulo de Ventas).</summary>
    public bool IsSellable { get; set; }

    //navegacion properties
    public virtual UnitMeasurement? UnitMeasurement { get; set; }
    public virtual CategoryProd? CategoryProduct { get; set; } = null!;
}
