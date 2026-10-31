using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class FixedAssetCategory
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    public decimal DefaultDepreciationRatePercent { get; set; }

    public int DefaultUsefulLifeYears { get; set; }
}
