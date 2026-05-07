namespace Biblioteca.Application.DTOs.Dashboard;

public sealed record DashboardKpisDto(
    BooksKpisDto Books,
    CopiesKpisDto Copies,
    UsersKpisDto Users,
    LoansKpisDto Loans,
    FinesKpisDto Fines,
    ReservationsKpisDto Reservations,
    IReadOnlyCollection<RecentActivityDto> RecentActivity);

public sealed record BooksKpisDto(int Total, int Active);

public sealed record CopiesKpisDto(
    int Total,
    int Available,
    int Loaned,
    int Maintenance);

public sealed record UsersKpisDto(int Total, int Active);

public sealed record LoansKpisDto(
    int Active,
    int Overdue,
    int TotalThisMonth,
    IReadOnlyCollection<LoanDailyKpiDto> Last30Days);

public sealed record LoanDailyKpiDto(DateOnly Date, int Total);

public sealed record FinesKpisDto(
    int Pending,
    decimal TotalAmountPendingMxn,
    int PaidThisMonth);

public sealed record ReservationsKpisDto(int Active, int Ready);

public sealed record RecentActivityDto(
    Guid Id,
    string TableName,
    string Action,
    Guid? RecordId,
    Guid? PerformedBy,
    DateTime PerformedAt,
    string Summary);
