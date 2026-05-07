namespace Biblioteca.Application.DTOs.Returns;

public sealed record ReturnDto(
    Guid Id,
    Guid LoanId,
    DateTime ReturnedAt,
    string Condition,
    string? InspectionNotes,
    Guid ReceivedByUserId,
    IReadOnlyCollection<FineDto> Fines);
