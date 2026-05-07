using Biblioteca.Application.DTOs.Dashboard;

namespace Biblioteca.Application.Interfaces.Dashboard;

public interface IDashboardRepository
{
    Task<BooksKpisDto> GetBooksAsync(CancellationToken ct);
    Task<CopiesKpisDto> GetCopiesAsync(CancellationToken ct);
    Task<UsersKpisDto> GetUsersAsync(CancellationToken ct);
    Task<LoansKpisDto> GetLoansAsync(DateTime now, DateTime monthStart, DateTime nextMonthStart, CancellationToken ct);
    Task<IReadOnlyCollection<LoanDailyKpiDto>> GetLoanDailyCountsAsync(DateTime from, DateTime to, CancellationToken ct);
    Task<FinesKpisDto> GetFinesAsync(DateTime monthStart, DateTime nextMonthStart, CancellationToken ct);
    Task<ReservationsKpisDto> GetReservationsAsync(CancellationToken ct);
    Task<IReadOnlyCollection<RecentActivityDto>> GetRecentActivityAsync(CancellationToken ct);
}
