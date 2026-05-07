using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Returns;

public interface IFineConfigRepository
{
    /// <summary>
    /// Devuelve la config con mayor EffectiveFrom que sea &lt;= ahora.
    /// </summary>
    Task<FineConfig?> GetActiveAsync(CancellationToken ct);
    Task<FineConfig> AddAsync(FineConfig config, CancellationToken ct);
}
