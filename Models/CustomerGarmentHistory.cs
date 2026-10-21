using DexterCompany.Models;
using System.ComponentModel.DataAnnotations;

public class CustomerGarmentHistory
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int ProductId { get; set; }
    public ProdService? Product { get; set; }
    [MaxLength(500)]
    public string Detail { get; set; } = "";
    [MaxLength(500)]
    public string NormalizedDetail { get; set; } = "";
    public int UsageCount { get; set; } = 1;
    public DateTime LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    [MaxLength(1)]
    public string Status { get; set; } = "A"; // A: Activo, I: Inactivo (depurado por el usuario)
}
