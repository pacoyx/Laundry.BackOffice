// Registro append-only: cada ping del chofer genera una fila nueva (igual criterio que
// LocationWorkGuide), en vez de sobreescribir una sola fila "ubicacion actual". Esto da de
// regalo el recorrido completo del dia por si se necesita mas adelante, sin cambiar el esquema.
public class DriverLocation
{
    public long Id { get; set; }

    public long EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double? AccuracyMeters { get; set; }

    // Hora del GPS en el celular del chofer (puede diferir un poco de CreatedAt si hubo
    // demora de red al momento de enviarla).
    public DateTime RecordedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
