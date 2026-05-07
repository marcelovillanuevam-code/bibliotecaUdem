namespace Biblioteca.Domain.Entities;

public enum ReservationStatus { PENDING, READY, FULFILLED, CANCELLED, EXPIRED }

public sealed class Reservation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public int QueuePosition { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public Guid? FulfilledByLoanId { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Usuario? User { get; set; }
    public Libro? Book { get; set; }
    public Loan? FulfilledByLoan { get; set; }
}
