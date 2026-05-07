namespace Biblioteca.Domain.Entities;

public sealed class Loan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BookCopyId { get; set; }
    public DateTime LoanedAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string Status { get; set; } = LoanStatus.Active;
    public int RenewalCount { get; set; }
    public Guid IssuedByUserId { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Usuario? User { get; set; }
    public BookCopy? BookCopy { get; set; }
    public Usuario? IssuedByUser { get; set; }
    public ICollection<LoanRenewal> Renewals { get; set; } = [];
}

public static class LoanStatus
{
    public const string Active = "ACTIVE";
    public const string Returned = "RETURNED";
    public const string Overdue = "OVERDUE";
    public const string Lost = "LOST";

    private static readonly HashSet<string> Valid = [Active, Returned, Overdue, Lost];

    public static bool IsValid(string status) =>
        Valid.Contains(status, StringComparer.OrdinalIgnoreCase);
}
