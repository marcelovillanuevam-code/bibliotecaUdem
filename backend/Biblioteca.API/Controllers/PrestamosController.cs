using Biblioteca.API.Extensions;
using Biblioteca.Application.DTOs.Prestamos;
using Biblioteca.Application.Features.Prestamos;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Loans;
using Biblioteca.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route(PrestamosFeature.Route)]
public sealed class PrestamosController(
    ILoanService loanService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanDto>> CreateAsync(
        [FromBody] CreateLoanRequest request,
        CancellationToken ct)
    {
        if (currentUserService.CurrentUserId is not { } issuedBy)
            return Unauthorized();

        var loan = await loanService.CreateAsync(request, issuedBy, ct);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = loan.Id }, loan);
    }

    [HttpGet("{id:guid}", Name = "GetLoanById")]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanDto>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var loan = await loanService.GetByIdAsync(id, ct);
        if (loan is null) return NotFound();

        var currentUserId = currentUserService.CurrentUserId;
        if (!User.IsInRole("ADMIN") && !User.IsInRole("LIBRARIAN") && loan.UserId != currentUserId)
            return Forbid();

        return Ok(loan);
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(IReadOnlyCollection<LoanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<LoanDto>>> GetAllAsync(
        [FromQuery] string? status,
        CancellationToken ct)
    {
        LoanStatus? statusFilter = null;
        if (status is not null && Enum.TryParse<LoanStatus>(status, ignoreCase: true, out var parsed))
            statusFilter = parsed;

        var loans = await loanService.GetAllAsync(statusFilter, ct);
        return Ok(loans);
    }

    [HttpPost("{id:guid}/renovaciones")]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanDto>> RenewAsync(Guid id, CancellationToken ct)
    {
        if (currentUserService.CurrentUserId is not { } requestedBy)
            return Unauthorized();

        var existing = await loanService.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        if (!User.IsInRole("ADMIN") && !User.IsInRole("LIBRARIAN") && existing.UserId != requestedBy)
            return Forbid();

        var loan = await loanService.RenewAsync(id, requestedBy, ct);
        return Ok(loan);
    }
}

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route("api/usuarios/{userId:guid}/prestamos")]
public sealed class UsuarioPrestamosController(
    ILoanService loanService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(IReadOnlyCollection<LoanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<LoanDto>>> GetByUserAsync(
        Guid userId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var currentUserId = currentUserService.CurrentUserId;
        if (!User.IsInRole("ADMIN") && !User.IsInRole("LIBRARIAN") && userId != currentUserId)
            return Forbid();

        LoanStatus? statusFilter = null;
        if (status is not null && Enum.TryParse<LoanStatus>(status, ignoreCase: true, out var parsed))
            statusFilter = parsed;

        var loans = await loanService.GetByUserAsync(userId, statusFilter, ct);
        return Ok(loans);
    }
}
