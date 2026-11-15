public enum EmployeeContractType
{
    Weekly = 1,
    Biweekly = 2,
    Monthly = 3,
    Payroll = 4,
    Services = 5,

    /// <summary>
    /// Trabajador ad honorem: se registra para control (asistencia, documentos) pero no recibe pago.
    /// Sueldo base siempre 0 y se excluye de la generación de planilla.
    /// </summary>
    AdHonorem = 6
}
