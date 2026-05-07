using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.Options;

public sealed class LoansOptions
{
    public const string SectionName = "Loans";

    [Required]
    public Dictionary<string, int> MaxActivePerRole { get; init; } = new();

    [Required]
    public Dictionary<string, int> DefaultDurationDaysPerRole { get; init; } = new();

    public int GetMaxActive(string role) =>
        MaxActivePerRole.TryGetValue(role, out var v) ? v : 3;

    public int GetDefaultDurationDays(string role) =>
        DefaultDurationDaysPerRole.TryGetValue(role, out var v) ? v : 14;
}
