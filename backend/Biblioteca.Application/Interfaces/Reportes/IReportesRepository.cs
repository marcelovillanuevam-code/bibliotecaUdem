using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Reportes;

public interface IReportesRepository
{
    Task<IReadOnlyCollection<Fine>> GetPaidFinesAsync(DateTime from, DateTime to, CancellationToken ct);
    Task<IReadOnlyCollection<Fine>> GetPendingFinesAsync(CancellationToken ct);
    Task<(int TotalDevoluciones, IReadOnlyCollection<int?> DiasRetraso)> GetDevolucionesTardiasDataAsync(DateTime from, DateTime to, CancellationToken ct);
    Task<IReadOnlyCollection<Fine>> GetWaivedFinesAsync(DateTime from, DateTime to, CancellationToken ct);
    Task<Dictionary<Guid, Usuario>> GetUsuariosByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
