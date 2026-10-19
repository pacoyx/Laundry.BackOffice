using System.ComponentModel.DataAnnotations;

public class Company
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string NameComercial { get; set; } = "";
    [MaxLength(200)]
    public string NameCompany { get; set; } = "";
    [MaxLength(5)]
    public string DocumentType { get; set; } = "";
    [MaxLength(20)]
    public string NumberType { get; set; } = "";
    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(100)]
    public string? Email { get; set; }
    [MaxLength(500)]
    public string? Logo { get; set; }
    [MaxLength(200)]
    public string? WebSite { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    [MaxLength(200)]
    public string? Address { get; set; }
    [MaxLength(1)]
    public string Status { get; set; } = "A";
    public int UsuarioId { get; set; }
    [MaxLength(200)]
    public string? Facebook { get; set; }
    [MaxLength(200)]
    public string? Twitter { get; set; }
    [MaxLength(200)]
    public string? Instagram { get; set; }

}