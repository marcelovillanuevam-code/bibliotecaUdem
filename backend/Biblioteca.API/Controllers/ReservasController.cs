using Biblioteca.API.Extensions;
using Biblioteca.Application.DTOs.Reservations;
using Biblioteca.Application.Features.Reservas;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route(ReservasFeature.Route)]
public sealed class ReservasController(
    IReservationService reservationService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> CreateAsync(
        [FromBody] CreateReservationRequest request,
        CancellationToken ct)
    {
        if (currentUserService.CurrentUserId is not { } userId)
            return Unauthorized();

        var reservation = await reservationService.CreateAsync(userId, request, ct);
        return CreatedAtRoute("GetReservationById", new { id = reservation.Id }, reservation);
    }

    [HttpGet("{id:guid}", Name = "GetReservationById")]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var reservation = await reservationService.GetByIdAsync(id, ct);
        if (reservation is null) return NotFound();

        var currentUserId = currentUserService.CurrentUserId;
        if (!User.IsInRole("ADMIN") && !User.IsInRole("LIBRARIAN") && reservation.UserId != currentUserId)
            return Forbid();

        return Ok(reservation);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelAsync(Guid id, CancellationToken ct)
    {
        if (currentUserService.CurrentUserId is not { } requestedBy)
            return Unauthorized();

        var isAdmin = User.IsInRole("ADMIN");
        await reservationService.CancelAsync(id, requestedBy, isAdmin, ct);
        return NoContent();
    }
}

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route("api/usuarios/{userId:guid}/reservas")]
public sealed class UsuarioReservasController(
    IReservationService reservationService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ReservationDto>>> GetByUserAsync(
        Guid userId,
        CancellationToken ct)
    {
        var currentUserId = currentUserService.CurrentUserId;
        if (!User.IsInRole("ADMIN") && !User.IsInRole("LIBRARIAN") && userId != currentUserId)
            return Forbid();

        var reservations = await reservationService.GetByUserAsync(userId, ct);
        return Ok(reservations);
    }
}

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route("api/libros/{libroId:guid}/reservas")]
public sealed class LibroReservasController(IReservationService reservationService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ReservationDto>>> GetQueueAsync(
        Guid libroId,
        CancellationToken ct)
    {
        var queue = await reservationService.GetQueueAsync(libroId, ct);
        return Ok(queue);
    }
}
