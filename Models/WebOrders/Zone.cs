using System.ComponentModel.DataAnnotations;

public class Zone
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<District> Districts { get; set; } = new List<District>();
}
