namespace Biblioteca.Domain.Entities;

public sealed class BookCopy
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public Guid? LocationId { get; set; }
    public string Status { get; set; } = BookCopyStatus.Available;
    public string? Condition { get; set; }
    public DateTime AcquiredAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Libro? Book { get; set; }
    public Ubicacion? Location { get; set; }
}

public static class BookCopyStatus
{
    public const string Available = "AVAILABLE";
    public const string Maintenance = "MAINTENANCE";
    public const string Lost = "LOST";
    public const string Retired = "RETIRED";

    // Reservados para Sprint 1 — no validar como inválidos para no remigrar luego.
    public const string Loaned = "LOANED";
    public const string Reserved = "RESERVED";

    private static readonly HashSet<string> ValidSprint0 =
        [Available, Maintenance, Lost, Retired, Loaned, Reserved];

    public static bool IsValid(string status) =>
        ValidSprint0.Contains(status, StringComparer.OrdinalIgnoreCase);
}

public static class BookCopyCondition
{
    public const string New = "NEW";
    public const string Good = "GOOD";
    public const string Worn = "WORN";
    public const string Damaged = "DAMAGED";

    private static readonly HashSet<string> Valid = [New, Good, Worn, Damaged];

    public static bool IsValid(string condition) =>
        Valid.Contains(condition, StringComparer.OrdinalIgnoreCase);
}
