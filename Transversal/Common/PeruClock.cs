public static class PeruClock
{
    private static readonly TimeSpan PeruUtcOffset = TimeSpan.FromHours(-5);

    public static DateTime Now => DateTime.UtcNow + PeruUtcOffset;

    // Convierte un DateTime "de reloj" en hora de Peru (ej. limites de un dia calendario)
    // al UTC con el que se guardan RecordedAt/CreatedAt en la base.
    public static DateTime ToUtc(DateTime peruLocal) => peruLocal - PeruUtcOffset;
}
