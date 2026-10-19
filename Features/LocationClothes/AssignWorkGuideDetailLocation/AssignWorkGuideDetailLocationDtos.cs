public record AssignWorkGuideDetailLocationRequestDto(
    int LocationClothesId,
    DateTime? FechaEntregaAprox,
    string? Comments
);

public record WorkGuideDetailLocationResponseDto(
    int WorkGuideDetailId,
    string Ubicacion,
    int LocationClothesId,
    string LocationClothesName,
    DateTime FechaEnvio,
    DateTime? FechaEntregaAprox,
    string? Comments
);
