using System.ComponentModel.DataAnnotations.Schema;
using DexterCompany.Models;

public class ZonePriceListDetail
{
    public long Id { get; set; }

    public long ZonePriceListId { get; set; }
    public ZonePriceList ZonePriceList { get; set; } = null!;

    public int ServiceItemId { get; set; }
    public ProdService ServiceItem { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
}
