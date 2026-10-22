// Traduce entre una expresion cron (lo que Hangfire necesita) y campos estructurados
// (Hour, Minute, DaysOfWeek) que el frontend puede renderizar sin tener que construir
// o interpretar sintaxis cron. ScheduledWeekday usa numeracion 1=Lunes...7=Domingo; el
// campo dia-de-semana de cron usa 0=Domingo...6=Sabado, por eso se traduce Domingo (7) <-> 0.
public static class CronScheduleHelper
{
    public static string BuildCronExpression(int hour, int minute, IEnumerable<ScheduledWeekday> daysOfWeek)
    {
        var days = daysOfWeek.Select(ToCronDayOfWeek).Distinct().OrderBy(d => d).ToList();
        if (days.Count == 0)
        {
            throw new ArgumentException("Debe seleccionar al menos un día de la semana.", nameof(daysOfWeek));
        }

        return $"{minute} {hour} * * {string.Join(",", days)}";
    }

    public static (int Hour, int Minute, List<ScheduledWeekday> DaysOfWeek) ParseCronExpression(string cronExpression)
    {
        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
        {
            throw new FormatException($"Expresión cron inválida: '{cronExpression}'.");
        }

        var minute = int.Parse(parts[0]);
        var hour = int.Parse(parts[1]);
        var daysOfWeek = parts[4]
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => FromCronDayOfWeek(int.Parse(d)))
            .Distinct()
            .OrderBy(d => (int)d)
            .ToList();

        return (hour, minute, daysOfWeek);
    }

    private static int ToCronDayOfWeek(ScheduledWeekday day) =>
        day == ScheduledWeekday.Sunday ? 0 : (int)day;

    private static ScheduledWeekday FromCronDayOfWeek(int cronDay) =>
        cronDay is 0 or 7 ? ScheduledWeekday.Sunday : (ScheduledWeekday)cronDay;
}
