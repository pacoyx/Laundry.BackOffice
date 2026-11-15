using System.ComponentModel.DataAnnotations;

/// <summary>
/// Descuento adicional ("Otros descuentos") de un trabajador en un periodo de planilla.
/// Se guarda por periodo y trabajador (no por planilla) para que sobreviva si la planilla se elimina y se vuelve a generar.
/// </summary>
public class PayrollDiscount : BaseAuditableEntity
{
    public long Id { get; set; }

    public long PayrollPeriodId { get; set; }

    public long EmployeeId { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PayrollPeriod? PayrollPeriod { get; set; }

    public Employee? Employee { get; set; }
}
