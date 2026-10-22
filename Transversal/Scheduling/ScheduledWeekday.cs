// Numeracion estilo ISO (1=Lunes...6=Sabado, 7=Domingo) para el contrato con el frontend.
// No coincide con el enum DayOfWeek de .NET (Sunday=0); CronScheduleHelper traduce internamente.
public enum ScheduledWeekday
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7
}
