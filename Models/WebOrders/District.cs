using System.ComponentModel.DataAnnotations;

public class District
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int ZoneId { get; set; }
    public Zone Zone { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
