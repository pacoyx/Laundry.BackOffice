using Microsoft.EntityFrameworkCore;

public static class WorkGuideTotalsRecalculator
{
    public static async Task RecalculateAsync(RecepcionDbContext db, int workGuideMainId)
    {
        var workGuideMain = await db.WorkGuideMains.FirstAsync(x => x.Id == workGuideMainId);

        var total = await db.WorkGuideDetails
            .Where(x => x.WorkGuideMainId == workGuideMainId && x.EstadoRegistro == "A")
            .SumAsync(x => x.Total);

        workGuideMain.Total = total;
        workGuideMain.Saldo = total - workGuideMain.Acuenta;
    }
}
