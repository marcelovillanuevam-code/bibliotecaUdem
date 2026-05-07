using Biblioteca.API.Extensions;
using Biblioteca.Application.DTOs.Returns;
using Biblioteca.Application.Features.Devoluciones;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route(DevolucionesFeature.MultasRoute)]
public sealed class MultasController(
    IFineService fineService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(IReadOnlyCollection<FineDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<FineDto>>> GetAllAsync(
        [FromQuery] Guid userId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        FineStatus? statusFilter = null;
        if (status is not null && Enum.TryParse<FineStatus>(status, ignoreCase: true, out var parsed))
            statusFilter = parsed;

        var fines = await fineService.GetByUserAsync(userId, statusFilter, ct);
        return Ok(fines);
    }

    [HttpGet("{id:guid}", Name = "GetFineById")]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(FineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FineDto>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var currentUserId = currentUserService.CurrentUserId ?? Guid.Empty;
        var isAdmin = User.IsInRole("ADMIN") || User.IsInRole("LIBRARIAN");
        var fine = await fineService.GetByIdAsync(id, currentUserId, isAdmin, ct);
        if (fine is null) return NotFound();
        return Ok(fine);
    }

    [HttpPost("{id:guid}/pagos")]
    [Authorize(Policy = AuthPolicies.TreasuryOrAdmin)]
    [ProducesResponseType(typeof(FineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FineDto>> ConfirmPaymentAsync(
        Guid id,
        [FromBody] ConfirmPaymentRequest request,
        CancellationToken ct)
    {
        if (currentUserService.CurrentUserId is not { } paidBy)
            return Unauthorized();

        var fine = await fineService.ConfirmPaymentAsync(id, request, paidBy, ct);
        return Ok(fine);
    }

    [HttpPost("{id:guid}/condonar")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(typeof(FineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FineDto>> WaiveAsync(
        Guid id,
        [FromBody] WaiveFineRequest request,
        CancellationToken ct)
    {
        if (currentUserService.CurrentUserId is not { } waivedBy)
            return Unauthorized();

        var fine = await fineService.WaiveAsync(id, request, waivedBy, ct);
        return Ok(fine);
    }
}

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route("api/usuarios/{userId:guid}/multas")]
public sealed class UsuarioMultasController(
    IFineService fineService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(IReadOnlyCollection<FineDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<FineDto>>> GetByUserAsync(
        Guid userId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var currentUserId = currentUserService.CurrentUserId;
        if (!User.IsInRole("ADMIN") && !User.IsInRole("LIBRARIAN") && userId != currentUserId)
            return Forbid();

        FineStatus? statusFilter = null;
        if (status is not null && Enum.TryParse<FineStatus>(status, ignoreCase: true, out var parsed))
            statusFilter = parsed;

        var fines = await fineService.GetByUserAsync(userId, statusFilter, ct);
        return Ok(fines);
    }
}
