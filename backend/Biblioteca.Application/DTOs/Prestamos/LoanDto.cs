namespace Biblioteca.Application.DTOs.Prestamos;

public sealed record LoanDto(
    Guid Id,
    Guid UserId,
    string UserFullName,
    Guid BookCopyId,
    string BookTitle,
    string Isbn,
    DateTime LoanedAt,
    DateTime DueAt,
    DateTime? ReturnedAt,
    string Status,
    int RenewalCount,
    IReadOnlyCollection<LoanRenewalDto> Renewals,
    string CopyBarcode,
    string BorrowerName,
    string BorrowerEmail);
