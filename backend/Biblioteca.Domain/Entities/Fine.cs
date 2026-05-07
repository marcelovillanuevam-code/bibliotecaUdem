namespace Biblioteca.Domain.Entities;

public enum FineReason { LATE, DAMAGE, LOSS }
public enum FineStatus { PENDING, PAID, WAIVED }

public sealed class Fine
{
    public Guid Id { get; set; }
    public Guid ReturnId { get; set; }
    public Guid UserId { get; set; }
    public FineReason Reason { get; set; }
    public decimal Amount { get; set; }
    public int? DaysLate { get; set; }
    public FineStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? PaidByUserId { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Return? Return { get; set; }
    public Usuario? User { get; set; }
}
