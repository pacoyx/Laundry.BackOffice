using Microsoft.EntityFrameworkCore;

public class DeleteUnitMeasurementService : IDeleteUnitMeasurementService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<DeleteUnitMeasurementService> Logger { get; }

    public DeleteUnitMeasurementService(RecepcionDbContext context, IAppLogger<DeleteUnitMeasurementService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public async Task<string> DeleteUnitMeasurementAsync(int id)
    {
        var unitMeasurement = await _context.UnitMeasurements.FirstOrDefaultAsync(x => x.Id == id);
        if (unitMeasurement == null)
        {
            return $"Unidad de medida con ID {id} no encontrada.";
        }

        // Baja lógica: Product tiene FK obligatoria a UnitMeasurementId, un delete físico
        // rompería la integridad referencial (o arrastraría productos en cascada).
        unitMeasurement.Status = "I";

        await _context.SaveChangesAsync();

        return "OK";
    }
}
