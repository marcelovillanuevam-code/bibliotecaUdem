namespace Biblioteca.Application.DTOs.Returns;

public sealed record FineConfigDto(
    Guid Id,
    decimal LateRatePerDayMxn,
    decimal DamageFlatMxn,
    decimal LossFlatMxn,
    DateTime EffectiveFrom);
