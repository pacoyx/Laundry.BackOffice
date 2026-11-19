using Microsoft.EntityFrameworkCore;

public class UpdateUnitMeasurementService : IUpdateUnitMeasurementService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<UpdateUnitMeasurementService> Logger { get; }

    public UpdateUnitMeasurementService(RecepcionDbContext context, IAppLogger<UpdateUnitMeasurementService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public async Task<string> UpdateUnitMeasurementAsync(UpdateUnitMeasurementRequestDto request)
    {
        var unitMeasurement = await _context.UnitMeasurements.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (unitMeasurement == null)
        {
            return $"Unidad de medida con ID {request.Id} no encontrada.";
        }

        unitMeasurement.Name = request.Name;
        unitMeasurement.CodeUm = request.CodeUm;
        unitMeasurement.Description = request.Description;
        unitMeasurement.Status = request.Status;

        await _context.SaveChangesAsync();

        return "OK";
    }
}
