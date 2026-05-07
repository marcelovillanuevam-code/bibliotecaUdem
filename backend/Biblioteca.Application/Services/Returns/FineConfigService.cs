using Biblioteca.Application.DTOs.Returns;
using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Services.Returns;

public sealed class FineConfigService(IFineConfigRepository fineConfigRepository) : IFineConfigService
{
    public async Task<FineConfigDto?> GetActiveAsync(CancellationToken ct)
    {
        var config = await fineConfigRepository.GetActiveAsync(ct);
        return config is null ? null : MapToDto(config);
    }

    public async Task<FineConfigDto> UpdateAsync(UpdateFineConfigRequest request, Guid updatedBy, CancellationToken ct)
    {
        var config = new FineConfig
        {
            Id = Guid.NewGuid(),
            LateRatePerDayMxn = request.LateRatePerDayMxn,
            DamageFlatMxn = request.DamageFlatMxn,
            LossFlatMxn = request.LossFlatMxn,
            EffectiveFrom = request.EffectiveFrom.Kind == DateTimeKind.Utc
                ? request.EffectiveFrom
                : DateTime.SpecifyKind(request.EffectiveFrom, DateTimeKind.Utc)
        };

        await fineConfigRepository.AddAsync(config, ct);
        return MapToDto(config);
    }

    private static FineConfigDto MapToDto(FineConfig c) =>
        new(c.Id, c.LateRatePerDayMxn, c.DamageFlatMxn, c.LossFlatMxn, c.EffectiveFrom);
}
