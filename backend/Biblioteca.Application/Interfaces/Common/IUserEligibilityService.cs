namespace Biblioteca.Application.Interfaces.Common;

public sealed record EligibilityResult(bool IsEligible, string? Reason);

public interface IUserEligibilityService
{
    Task<EligibilityResult> CheckAsync(Guid userId, CancellationToken ct);
}
