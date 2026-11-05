public record ServiceAccessFastAdminDto(
    int Id,
    int ProdServiceId,
    string ProdServiceName,
    string ShortName,
    string IconName,
    string Status,
    int Order
);

public record CreateServiceAccessFastRequest(
    int ProdServiceId,
    string ShortName,
    string? IconName,
    string? Status
);

public record UpdateServiceAccessFastRequest(
    int ProdServiceId,
    string ShortName,
    string? IconName,
    string? Status
);

// El orden final de cada Id es su posicion dentro de la lista (1-based).
public record ReorderServiceAccessFastRequest(
    List<int> OrderedIds
);
