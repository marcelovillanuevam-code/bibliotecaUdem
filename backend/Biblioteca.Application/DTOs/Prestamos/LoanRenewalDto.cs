namespace Biblioteca.Application.DTOs.Prestamos;

public sealed record LoanRenewalDto(
    Guid Id,
    Guid LoanId,
    DateTime RenewedAt,
    DateTime PreviousDueAt,
    DateTime NewDueAt,
    Guid RenewedByUserId);
