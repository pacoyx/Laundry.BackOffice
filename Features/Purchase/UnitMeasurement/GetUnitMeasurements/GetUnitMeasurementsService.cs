
using Microsoft.EntityFrameworkCore;

public class GetUnitMeasurementsService : IGetUnitMeasurementsService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<GetUnitMeasurementsService> Logger { get; }

    public GetUnitMeasurementsService(RecepcionDbContext context, IAppLogger<GetUnitMeasurementsService> logger)
    {
        _context = context;
        Logger = logger;
    }

    async public Task<IEnumerable<GetUnitMeasurementsResponseDto>> GetUnitMeasurementsAsync()
    {
        var unitMeasurements = await _context.UnitMeasurements
        .Select(x => new GetUnitMeasurementsResponseDto
        {
            Id = x.Id,
            CodeUm = x.CodeUm,
            Name = x.Name,
            Description = x.Description,
            Status = x.Status
        }).ToListAsync();

        return unitMeasurements;
    }

    async public Task<IEnumerable<GetUnitMeasurementsComboResponseDto>> GetUnitMeasurementsComboAsync()
    {
        var unitMeasurements = await _context.UnitMeasurements
        .Select(x => new GetUnitMeasurementsComboResponseDto
        {
            Id = x.Id,
            CodeUm = x.CodeUm,
            Name = x.Name
        }).ToListAsync();

        return unitMeasurements;
    }
}