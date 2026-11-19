
using Microsoft.EntityFrameworkCore;

public class GetSupplierService : IGetSupplierService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<GetSupplierService> Logger { get; }

    public GetSupplierService(RecepcionDbContext context, IAppLogger<GetSupplierService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public Task<GetSupplierResponseDto?> GetSupplierAsync(int id)
    {
        var supplier = _context.Suppliers.Find(id);
        if (supplier == null)
        {
            Logger.LogWarning($"Supplier with id {id} not found.");
            return Task.FromResult<GetSupplierResponseDto?>(null);
        }

        return Task.FromResult<GetSupplierResponseDto?>(new GetSupplierResponseDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Ruc = supplier.Ruc,
            Address = supplier.Address,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Status = supplier.Status,
            ContactName = supplier.ContactName,
            ContactPhone = supplier.ContactPhone
        });
    }

    public Task<IEnumerable<GetSupplierResponseDto>> GetSuppliersAsync()
    {
        var suppliers = _context.Suppliers.ToList();
        if (suppliers == null || !suppliers.Any())
        {
            Logger.LogWarning("No suppliers found.");
            return Task.FromResult<IEnumerable<GetSupplierResponseDto>>(Enumerable.Empty<GetSupplierResponseDto>());
        }

        return Task.FromResult<IEnumerable<GetSupplierResponseDto>>(suppliers.Select(supplier => new GetSupplierResponseDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Ruc = supplier.Ruc,
            Address = supplier.Address,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Status = supplier.Status,
            ContactName = supplier.ContactName,
            ContactPhone = supplier.ContactPhone
        }).ToList());
    }

    async public Task<GetSupplierResponsePaginatorDto> GetSuppliersPaginatorAsync(int pageNumber, int pageSize, string name)
    {
        var query = _context.Suppliers.AsNoTracking();
        var totalRows = await query.CountAsync();

        if (name != null && !string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.Name.Contains(name));
        }

        var suppliers = query
            .OrderBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        if (suppliers == null || !suppliers.Any())
        {
            Logger.LogWarning("No suppliers found.");
            return new GetSupplierResponsePaginatorDto(0, Enumerable.Empty<GetSupplierResponseDto>().ToList());
        }

        var response = new GetSupplierResponsePaginatorDto(totalRows, suppliers.Select(supplier => new GetSupplierResponseDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Ruc = supplier.Ruc,
            Address = supplier.Address,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Status = supplier.Status,
            ContactName = supplier.ContactName,
            ContactPhone = supplier.ContactPhone
        }).ToList());

        return response;
    }

    public async Task<IEnumerable<GetSupplierSearchPatronResponseDto>> GetSuppliersByPatronAsync(string patronName)
    {        
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.Name.Contains(patronName))
            .OrderBy(s => s.Name)
            .ToListAsync();

        if (suppliers == null || !suppliers.Any())
        {
            Logger.LogWarning($"No suppliers found for patron name: {patronName}.");
            return Enumerable.Empty<GetSupplierSearchPatronResponseDto>();
        }

        var response = suppliers.Select(s => new GetSupplierSearchPatronResponseDto(s.Id, s.Name, s.Ruc)).ToList();

        return response;

    }
}