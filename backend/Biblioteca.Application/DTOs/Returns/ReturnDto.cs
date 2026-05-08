namespace Biblioteca.Application.DTOs.Returns;

public sealed record ReturnDto(
    Guid Id,
    Guid LoanId,
    DateTime ReturnedAt,
    string Condition,
    string? InspectionNotes,
    Guid ReceivedByUserId,
    string? BookTitle,
    string? CopyBarcode,
    string? BorrowerName,
    string? BorrowerEmail,
    IReadOnlyCollection<FineDto> Fines);
