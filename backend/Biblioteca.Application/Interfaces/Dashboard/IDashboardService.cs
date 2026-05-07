using Biblioteca.Application.DTOs.Dashboard;

namespace Biblioteca.Application.Interfaces.Dashboard;

public interface IDashboardService
{
    Task<DashboardKpisDto> GetKpisAsync(CancellationToken ct);
}
