using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class PurchaseInvoice
{
    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.Now;
    public int SupplierId { get; set; }
    [MaxLength(5)]
    public string InvoiceType { get; set; } = string.Empty;
    [MaxLength(10)]
    public string InvoiceSerie { get; set; } = string.Empty;
    [MaxLength(20)]
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceIssueDate { get; set; } = DateTime.Now;
    public DateTime InvoiceExpirationDate { get; set; } = DateTime.Now;
    [MaxLength(10)]
    public string TypePayment { get; set; } = string.Empty; // contado / credito    
    public int DaysCredit { get; set; } = 0; // Number of days for credit payment
    [MaxLength(100)]
    public string PaymentMethod { get; set; } = string.Empty; // e.g., "Cash", "Credit Card"
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
    [MaxLength(1)]
    public string Status { get; set; } = "A";
    [MaxLength(3)]
    public string CurrencyId { get; set; } = "";
    [Column(TypeName = "decimal(18,4)")]
    public decimal ExchangeRate { get; set; } = 1.0m; 
    [MaxLength(200)]
    public string Comments { get; set; } = "";
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Igv { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
    public PurchaseInvoiceStatus PaymentStatus { get; set; } = PurchaseInvoiceStatus.Pending;

    /// <summary>
    /// Almacén donde ingresan los productos comprados. Null en compras anteriores al módulo de
    /// inventarios (no generan movimiento).
    /// </summary>
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }


    // Navigation properties
    public ICollection<PurchaseInvoiceDetails>? PurchaseInvoiceDetails { get; set; }
    public Supplier? Supplier { get; set; }
    public ICollection<PurchaseInvoicePayment> Payments { get; set; } = new List<PurchaseInvoicePayment>();
}