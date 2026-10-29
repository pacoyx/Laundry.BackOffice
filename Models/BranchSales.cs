using DexterCompany.Models;

public class BranchSales
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<BrachSalesUser> BrachSalesUsers { get; set; } = [];
    public string Code { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public bool IsCentral { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public int DefaultPrinterId { get; set; }
    public ICollection<WorkerNode> WorkerNodes { get; set; } = new List<WorkerNode>();
}