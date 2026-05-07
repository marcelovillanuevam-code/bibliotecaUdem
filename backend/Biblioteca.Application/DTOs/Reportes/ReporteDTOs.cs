namespace Biblioteca.Application.DTOs.Reportes;

public sealed record PorMesDto(string Mes, decimal Total, int Conteo);
public sealed record PorMotivoDto(string Motivo, decimal Total, int Conteo);

public sealed record MultasRecaudadasDto(
    decimal TotalGeneral,
    int TotalConteo,
    IReadOnlyCollection<PorMesDto> PorMes,
    IReadOnlyCollection<PorMotivoDto> PorMotivo);

public sealed record DeudorDto(
    Guid UserId,
    string NombreUsuario,
    string EmailUsuario,
    int CantidadMultas,
    decimal TotalPendiente);

public sealed record DevolucionesTardiasDto(
    int TotalDevoluciones,
    int TotalTardias,
    double PorcentajeTardias,
    double? PromedioDiasRetraso);

public sealed record CondonacionDto(
    Guid Id,
    Guid DeudorId,
    string NombreDeudor,
    string EmailDeudor,
    string Motivo,
    string? MotivoCondonacion,
    decimal Monto,
    DateTime CondonadaEn,
    Guid? CondonadaPorId,
    string NombreCondonador);
