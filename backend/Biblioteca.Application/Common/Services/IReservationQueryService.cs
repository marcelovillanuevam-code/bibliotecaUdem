namespace Biblioteca.Application.Common.Services;

/// <summary>
/// Consultas de reservas que otros módulos necesitan sin acceder directamente al repositorio.
/// Marcelo lo consume en la validación de renovaciones de préstamos.
/// </summary>
public interface IReservationQueryService
{
    Task<bool> HasActiveReservationsForBookAsync(Guid bookId, CancellationToken ct);
}
