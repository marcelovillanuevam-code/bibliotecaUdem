using Biblioteca.Application.DTOs.Returns;

namespace Biblioteca.Application.Interfaces.Returns;

public interface IFineConfigService
{
    Task<FineConfigDto?> GetActiveAsync(CancellationToken ct);
    Task<FineConfigDto> UpdateAsync(UpdateFineConfigRequest request, Guid updatedBy, CancellationToken ct);
}
