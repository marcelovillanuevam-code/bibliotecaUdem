namespace Biblioteca.Application.DTOs.Reservations;

public sealed record ReservationDto(
    Guid Id,
    Guid UserId,
    Guid BookId,
    string BookTitle,
    int QueuePosition,
    string Status,
    DateTime CreatedAt,
    DateTime? ReadyAt,
    DateTime? ExpiresAt,
    DateTime? FulfilledAt,
    Guid? FulfilledByLoanId);
