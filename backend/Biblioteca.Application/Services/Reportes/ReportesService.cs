using Biblioteca.Application.DTOs.Reportes;
using Biblioteca.Application.Interfaces.Reportes;

namespace Biblioteca.Application.Services.Reportes;

public sealed class ReportesService(IReportesRepository repo) : IReportesService
{
    public async Task<MultasRecaudadasDto> GetMultasRecaudadasAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var (fromDt, toDt) = ToUtcRange(from, to);
        var fines = await repo.GetPaidFinesAsync(fromDt, toDt, ct);

        var porMes = fines
            .GroupBy(f => f.PaidAt!.Value.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new PorMesDto(g.Key, g.Sum(f => f.Amount), g.Count()))
            .ToList();

        var porMotivo = fines
            .GroupBy(f => f.Reason.ToString())
            .OrderBy(g => g.Key)
            .Select(g => new PorMotivoDto(g.Key, g.Sum(f => f.Amount), g.Count()))
            .ToList();

        return new MultasRecaudadasDto(fines.Sum(f => f.Amount), fines.Count, porMes, porMotivo);
    }

    public async Task<IReadOnlyCollection<DeudorDto>> GetMultasPendientesAsync(CancellationToken ct)
    {
        var fines = await repo.GetPendingFinesAsync(ct);

        return fines
            .GroupBy(f => f.UserId)
            .Select(g =>
            {
                var user = g.First().User;
                var nombre = user?.Profile is { } p
                    ? $"{p.FirstName} {p.LastName}".Trim()
                    : g.Key.ToString();
                var email = user?.Contacts
                    .FirstOrDefault(c => c.Type == "email" && c.IsPrimary)?.Value ?? "";
                return new DeudorDto(g.Key, nombre, email, g.Count(), g.Sum(f => f.Amount));
            })
            .OrderByDescending(d => d.TotalPendiente)
            .ToList();
    }

    public async Task<DevolucionesTardiasDto> GetDevolucionesTardiasAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var (fromDt, toDt) = ToUtcRange(from, to);
        var (total, dias) = await repo.GetDevolucionesTardiasDataAsync(fromDt, toDt, ct);

        var tardias = dias.Count;
        var pct = total > 0 ? Math.Round((double)tardias / total * 100, 1) : 0.0;
        var promedio = tardias > 0
            ? (double?)Math.Round(dias.Where(d => d.HasValue).Average(d => d!.Value), 1)
            : null;

        return new DevolucionesTardiasDto(total, tardias, pct, promedio);
    }

    public async Task<IReadOnlyCollection<CondonacionDto>> GetCondonacionesAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var (fromDt, toDt) = ToUtcRange(from, to);
        var fines = await repo.GetWaivedFinesAsync(fromDt, toDt, ct);

        var condonadorIds = fines
            .Where(f => f.PaidByUserId.HasValue)
            .Select(f => f.PaidByUserId!.Value)
            .Distinct()
            .ToList();

        var condonadores = await repo.GetUsuariosByIdsAsync(condonadorIds, ct);

        return fines.Select(f =>
        {
            var deudor = f.User;
            var nombreDeudor = deudor?.Profile is { } dp
                ? $"{dp.FirstName} {dp.LastName}".Trim()
                : f.UserId.ToString();
            var emailDeudor = deudor?.Contacts
                .FirstOrDefault(c => c.Type == "email" && c.IsPrimary)?.Value ?? "";

            var nombreCondonador = "";
            if (f.PaidByUserId.HasValue
                && condonadores.TryGetValue(f.PaidByUserId.Value, out var cond)
                && cond.Profile is { } cp)
                nombreCondonador = $"{cp.FirstName} {cp.LastName}".Trim();

            return new CondonacionDto(
                f.Id,
                f.UserId,
                nombreDeudor,
                emailDeudor,
                f.Reason.ToString(),
                f.WaivedReason,
                f.Amount,
                f.PaidAt!.Value,
                f.PaidByUserId,
                nombreCondonador);
        }).ToList();
    }

    private static (DateTime From, DateTime To) ToUtcRange(DateOnly from, DateOnly to)
    {
        var fromDt = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toDt = DateTime.SpecifyKind(to.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        return (fromDt, toDt);
    }
}
