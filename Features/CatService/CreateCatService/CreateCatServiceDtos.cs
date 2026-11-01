
public record CreateCatServiceDto(
     string Name,
     string Description,
     string Icon
);

public record CatServiceResponseDto(
     int Id,
     string Name,
     string Description,
     string Icon
);
