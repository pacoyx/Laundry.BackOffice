using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebOrderStatus
{
    Pendiente = 1,
    Confirmado = 2,
    Programado = 3,
    Recogido = 4,
    Rechazado = 5,
    Cancelado = 6
}
