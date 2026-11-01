using System.ComponentModel.DataAnnotations;

public class ElectronicInvoiceSubmission
{
    public long Id { get; set; }

    public long InvoiceId { get; set; }

    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool Success { get; set; }

    public bool? AcceptedBySunat { get; set; }

    [MaxLength(500)]
    public string? SunatDescription { get; set; }

    [MaxLength(50)]
    public string? SunatResponseCode { get; set; }

    [MaxLength(500)]
    public string? DocumentUrl { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    public string? RawResponseJson { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
