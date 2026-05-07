using Biblioteca.Application.DTOs.Reportes;

namespace Biblioteca.Application.Interfaces.Reportes;

public interface IReportesService
{
    Task<MultasRecaudadasDto> GetMultasRecaudadasAsync(DateOnly from, DateOnly to, CancellationToken ct);
    Task<IReadOnlyCollection<DeudorDto>> GetMultasPendientesAsync(CancellationToken ct);
    Task<DevolucionesTardiasDto> GetDevolucionesTardiasAsync(DateOnly from, DateOnly to, CancellationToken ct);
    Task<IReadOnlyCollection<CondonacionDto>> GetCondonacionesAsync(DateOnly from, DateOnly to, CancellationToken ct);
}
