using System.ComponentModel.DataAnnotations.Schema;
using DexterCompany.Models;

public class WebOrderDetail
{
    public long Id { get; set; }

    public long WebOrderId { get; set; }
    public WebOrder WebOrder { get; set; } = null!;

    public int ServiceItemId { get; set; }
    public ProdService ServiceItem { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
}
