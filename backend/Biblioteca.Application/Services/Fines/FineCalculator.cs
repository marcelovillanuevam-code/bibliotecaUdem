using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Services.Fines;

public sealed record FineDraft(FineReason Reason, decimal Amount, int? DaysLate);

public sealed class FineCalculator(IFineConfigRepository fineConfigRepository)
{
    public async Task<List<FineDraft>> CalculateAsync(Loan loan, ReturnCondition condition, DateTime returnedAt, CancellationToken ct)
    {
        var config = await fineConfigRepository.GetActiveAsync(ct)
            ?? throw new InvalidOperationException("No hay configuración de multas activa.");

        var drafts = new List<FineDraft>();

        if (condition == ReturnCondition.LOST)
        {
            drafts.Add(new FineDraft(FineReason.LOSS, config.LossFlatMxn, null));
            return drafts;
        }

        var daysLate = (int)Math.Max(0, Math.Floor((returnedAt - loan.DueAt).TotalDays));

        if (daysLate > 0)
            drafts.Add(new FineDraft(FineReason.LATE, daysLate * config.LateRatePerDayMxn, daysLate));

        if (condition == ReturnCondition.DAMAGED)
            drafts.Add(new FineDraft(FineReason.DAMAGE, config.DamageFlatMxn, null));

        return drafts;
    }
}
