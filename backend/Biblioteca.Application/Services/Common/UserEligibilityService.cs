using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Returns;

namespace Biblioteca.Application.Services.Common;

public sealed class UserEligibilityService(IFineRepository fineRepository) : IUserEligibilityService
{
    public async Task<EligibilityResult> CheckAsync(Guid userId, CancellationToken ct)
    {
        var hasPending = await fineRepository.HasPendingByUserAsync(userId, ct);
        return hasPending
            ? new EligibilityResult(false, "El usuario tiene multas pendientes de pago.")
            : new EligibilityResult(true, null);
    }
}
