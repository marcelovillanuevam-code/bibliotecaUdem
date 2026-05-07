namespace Biblioteca.Domain.Entities;

public sealed class LoanRenewal
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public DateTime RenewedAt { get; set; }
    public DateTime PreviousDueAt { get; set; }
    public DateTime NewDueAt { get; set; }
    public Guid RenewedByUserId { get; set; }

    public Loan? Loan { get; set; }
    public Usuario? RenewedByUser { get; set; }
}
