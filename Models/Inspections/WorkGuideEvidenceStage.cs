using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkGuideEvidenceStage
{
    Recojo = 1,
    Revision = 2,
    Entrega = 3
}
