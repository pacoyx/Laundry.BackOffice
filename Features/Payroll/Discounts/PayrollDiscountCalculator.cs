/// <summary>
/// Reglas compartidas entre la generación de planilla y el recálculo por descuentos adicionales.
/// </summary>
public static class PayrollDiscountCalculator
{
    public static PayrollDetail ToDetail(PayrollDiscount discount) => new()
    {
        ConceptType = PayrollConceptType.OtherDiscount,
        Description = discount.Description,
        Amount = -discount.Amount
    };

    /// <summary>
    /// Recalcula totales a partir de las líneas: descuentos = suma de líneas negativas; neto = bruto - descuentos.
    /// </summary>
    public static void RecalculateTotals(Payroll payroll)
    {
        var details = payroll.PayrollDetails ?? new List<PayrollDetail>();
        payroll.Discounts = -details.Where(d => d.Amount < 0).Sum(d => d.Amount);
        payroll.NetAmount = payroll.GrossAmount - payroll.Discounts;
    }

    public static PayrollSummaryDto ToSummary(Payroll payroll, Employee employee) => new()
    {
        Id = payroll.Id,
        EmployeeId = payroll.EmployeeId,
        EmployeeName = $"{employee.FirstName} {employee.LastName}",
        GrossAmount = payroll.GrossAmount,
        Discounts = payroll.Discounts,
        NetAmount = payroll.NetAmount,
        GeneratedAt = payroll.GeneratedAt,
        Details = (payroll.PayrollDetails ?? new List<PayrollDetail>())
            .OrderBy(d => d.ConceptType).ThenBy(d => d.Id)
            .Select(d => new PayrollDetailLineDto
            {
                ConceptType = d.ConceptType,
                Description = d.Description,
                Amount = d.Amount
            }).ToList()
    };
}
