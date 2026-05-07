using Biblioteca.API.Extensions;
using Biblioteca.Application.DTOs.Notifications;
using Biblioteca.Application.Features.Reservas;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route(ReservasFeature.NotificacionesRoute)]
public sealed class NotificacionesController(
    INotificationService notificationService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(IReadOnlyCollection<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<NotificationDto>>> GetInboxAsync(
        [FromQuery] bool unreadOnly = false,
        CancellationToken ct = default)
    {
        if (currentUserService.CurrentUserId is not { } userId)
            return Unauthorized();

        var notifications = await notificationService.GetInboxAsync(userId, unreadOnly, ct);
        return Ok(notifications);
    }

    [HttpPatch("{id:guid}/leida")]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsReadAsync(Guid id, CancellationToken ct)
    {
        if (currentUserService.CurrentUserId is not { } userId)
            return Unauthorized();

        await notificationService.MarkAsReadAsync(id, userId, ct);
        return NoContent();
    }
}
