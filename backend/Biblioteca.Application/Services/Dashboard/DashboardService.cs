using Biblioteca.Application.DTOs.Dashboard;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Dashboard;

namespace Biblioteca.Application.Services.Dashboard;

public sealed class DashboardService(
    IDashboardRepository dashboardRepository,
    IDateTimeProvider clock) : IDashboardService
{
    public async Task<DashboardKpisDto> GetKpisAsync(CancellationToken ct)
    {
        var now = clock.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);
        var dailyFrom = now.Date.AddDays(-29);
        var dailyTo = now.Date.AddDays(1);

        var booksTask = dashboardRepository.GetBooksAsync(ct);
        var copiesTask = dashboardRepository.GetCopiesAsync(ct);
        var usersTask = dashboardRepository.GetUsersAsync(ct);
        var loansTask = dashboardRepository.GetLoansAsync(now, monthStart, nextMonthStart, ct);
        var loanDailyTask = dashboardRepository.GetLoanDailyCountsAsync(dailyFrom, dailyTo, ct);
        var finesTask = dashboardRepository.GetFinesAsync(monthStart, nextMonthStart, ct);
        var reservationsTask = dashboardRepository.GetReservationsAsync(ct);
        var activityTask = dashboardRepository.GetRecentActivityAsync(ct);

        await Task.WhenAll(
            booksTask,
            copiesTask,
            usersTask,
            loansTask,
            loanDailyTask,
            finesTask,
            reservationsTask,
            activityTask);

        var loans = await loansTask;
        var last30Days = FillLast30Days(await loanDailyTask, dailyFrom);

        return new DashboardKpisDto(
            await booksTask,
            await copiesTask,
            await usersTask,
            loans with { Last30Days = last30Days },
            await finesTask,
            await reservationsTask,
            await activityTask);
    }

    private static IReadOnlyCollection<LoanDailyKpiDto> FillLast30Days(
        IReadOnlyCollection<LoanDailyKpiDto> source,
        DateTime from)
    {
        var byDate = source.ToDictionary(item => item.Date, item => item.Total);

        return Enumerable.Range(0, 30)
            .Select(offset =>
            {
                var date = DateOnly.FromDateTime(from.AddDays(offset));
                return new LoanDailyKpiDto(date, byDate.GetValueOrDefault(date));
            })
            .ToList();
    }
}
