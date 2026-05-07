namespace Biblioteca.Application.DTOs.Returns;

public sealed record FineDto(
    Guid Id,
    Guid ReturnId,
    Guid UserId,
    string Reason,
    decimal Amount,
    int? DaysLate,
    string Status,
    DateTime CreatedAt,
    DateTime? PaidAt,
    Guid? PaidByUserId);
