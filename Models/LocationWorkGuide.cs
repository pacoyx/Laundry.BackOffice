public class LocationWorkGuide : AuditInfo
{
    public int Id { get; set; }
    public int LocationClothesId { get; set; }
    public int? WorkGuideId { get; set; }
    public int? WorkGuideDetailId { get; set; }
    public string? Comments { get; set; } = string.Empty;
    public string NumeroGuia { get; set; } = string.Empty;
    public LocationClothes? LocationClothes { get; set; }
    public bool IsSystem { get; set; }
    public DateTime? FechaEntregaAprox { get; set; }
}