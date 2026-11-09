using System.ComponentModel.DataAnnotations;

public class CompanyWorker : BaseAuditableEntity
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Area { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
