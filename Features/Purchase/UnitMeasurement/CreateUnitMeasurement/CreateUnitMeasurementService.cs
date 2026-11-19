public class CreateUnitMeasurementService : ICreateUnitMeasurementService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<CreateUnitMeasurementService> Logger { get; }

    public CreateUnitMeasurementService(RecepcionDbContext context, IAppLogger<CreateUnitMeasurementService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public async Task<CreateUnitMeasurementResponseDto> CreateUnitMeasurementAsync(CreateUnitMeasurementRequestDto request)
    {
        this.Logger.LogInformacion("EXEC CreateUnitMeasurementAsync()");

        var unitMeasurement = new UnitMeasurement
        {
            Name = request.Name,
            CodeUm = request.CodeUm,
            Description = request.Description,
            Status = request.Status
        };

        await _context.UnitMeasurements.AddAsync(unitMeasurement);
        await _context.SaveChangesAsync();

        var response = new CreateUnitMeasurementResponseDto
        {
            Id = unitMeasurement.Id,
            CodeUm = unitMeasurement.CodeUm,
            Name = unitMeasurement.Name,
            Description = unitMeasurement.Description,
            Status = unitMeasurement.Status
        };

        return response;
    }

}