using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class FixedAssetMaintenance
{
    public long Id { get; set; }

    public long FixedAssetId { get; set; }

    public DateTime Date { get; set; }

    public FixedAssetMaintenanceType Type { get; set; }

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; }

    [MaxLength(500)]
    public string? Observations { get; set; }

    public DateTime? NextMaintenanceDate { get; set; }

    [MaxLength(150)]
    public string? ServiceProvider { get; set; }

    // --- Parada del activo (estado "En Mantenimiento") ---
    // Si es true, al registrar el mantenimiento el activo pasa a InMaintenance
    // desde Date (fecha de inicio) hasta que se finalice (ActualEndDate).
    public bool PutsAssetOutOfService { get; set; }

    [MaxLength(300)]
    public string? OutOfServiceReason { get; set; }

    public DateTime? EstimatedEndDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    [MaxLength(500)]
    public string? ClosingNotes { get; set; }

    // Estado que tenia el activo antes de entrar en mantenimiento; se restaura al finalizar.
    public FixedAssetStatus? PreviousStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public FixedAsset FixedAsset { get; set; } = null!;

    public ICollection<FixedAssetMaintenanceAttachment> Attachments { get; set; } = new List<FixedAssetMaintenanceAttachment>();
}
