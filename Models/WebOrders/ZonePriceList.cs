public class ZonePriceList
{
    public long Id { get; set; }

    public int ZoneId { get; set; }
    public Zone Zone { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public int Version { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<ZonePriceListDetail> Details { get; set; } = new List<ZonePriceListDetail>();
}
