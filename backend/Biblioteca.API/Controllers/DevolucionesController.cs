using Biblioteca.API.Extensions;
using Biblioteca.Application.DTOs.Returns;
using Biblioteca.Application.Features.Devoluciones;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Returns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route(DevolucionesFeature.Route)]
public sealed class DevolucionesController(
    IReturnService returnService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReturnDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ReturnDto>>> ListAsync(CancellationToken ct)
    {
        var results = await returnService.ListAsync(ct);
        return Ok(results);
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(ReturnDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReturnDto>> CreateAsync(
        [FromBody] CreateReturnRequest request,
        CancellationToken ct)
    {
        if (currentUserService.CurrentUserId is not { } receivedBy)
            return Unauthorized();

        if (request.LoanId == Guid.Empty)
            return BadRequest(new ProblemDetails
            {
                Detail = "El campo loanId es obligatorio y debe ser un GUID válido."
            });

        var result = await returnService.CreateAsync(request, receivedBy, ct);
        return CreatedAtRoute("GetReturnById", new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}", Name = "GetReturnById")]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(ReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReturnDto>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var result = await returnService.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }
}
