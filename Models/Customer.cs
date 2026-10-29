
using System.ComponentModel.DataAnnotations;

public class Customer
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string FirtsName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? LastName { get; set; }
    [MaxLength(200)]
    public string? Address { get; set; }
    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(100)]
    public string? Email { get; set; }
    [MaxLength(20)]
    public string? DocPersonal { get; set; }
    [MaxLength(1)]
    public string Status { get; set; } = "A";

    public CustomerType CustomerType { get; set; } = CustomerType.Individual;

    [MaxLength(20)]
    public string? InternalCode { get; set; }

    public bool IsActive { get; set; } = true;

    public CustomerOrigin Origin { get; set; } = CustomerOrigin.Recepcion;

    public int? DistrictId { get; set; }
    public District? District { get; set; }

    public bool PortalEnabled { get; set; } = false;

    public CustomerBillingProfile? BillingProfile { get; set; }

    public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
}