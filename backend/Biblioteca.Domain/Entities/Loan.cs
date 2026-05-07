namespace Biblioteca.Domain.Entities;

public enum LoanStatus { ACTIVE, RETURNED, OVERDUE, LOST }

public sealed class Loan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BookCopyId { get; set; }
    public DateTime LoanedAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public LoanStatus Status { get; set; }
    public int RenewalCount { get; set; }
    public Guid IssuedByUserId { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Usuario? User { get; set; }
    public BookCopy? BookCopy { get; set; }
    public ICollection<LoanRenewal> Renewals { get; set; } = [];
}
