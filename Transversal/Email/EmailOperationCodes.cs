public static class EmailOperationCodes
{
    public const string CorporateOperations = "ope_corporativas";
    public const string InternalAlerts = "alertas_internas";
    public const string ExternalAlerts = "alertas_externas";
    public const string AutomatedTasks = "tareas_automatizadas";

    // Lista inicial cerrada: la columna Operacion sigue siendo string (no enum) para poder
    // agregar valores nuevos sin migración, pero por ahora solo se permiten estos 4.
    public static readonly IReadOnlyCollection<string> All = new[]
    {
        CorporateOperations,
        InternalAlerts,
        ExternalAlerts,
        AutomatedTasks
    };
}
