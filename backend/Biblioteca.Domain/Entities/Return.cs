namespace Biblioteca.Domain.Entities;

public enum ReturnCondition { OK, DAMAGED, LOST }

public sealed class Return
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public DateTime ReturnedAt { get; set; }
    public ReturnCondition Condition { get; set; }
    public string? InspectionNotes { get; set; }
    public Guid ReceivedByUserId { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Loan? Loan { get; set; }
    public Fine? Fine { get; set; }
}
