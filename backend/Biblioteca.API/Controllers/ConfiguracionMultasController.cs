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
[Route(DevolucionesFeature.ConfiguracionMultasRoute)]
public sealed class ConfiguracionMultasController(
    IFineConfigService fineConfigService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(FineConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FineConfigDto>> GetActiveAsync(CancellationToken ct)
    {
        var config = await fineConfigService.GetActiveAsync(ct);
        if (config is null) return NotFound();
        return Ok(config);
    }

    [HttpPut]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(typeof(FineConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FineConfigDto>> UpdateAsync(
        [FromBody] UpdateFineConfigRequest request,
        CancellationToken ct)
    {
        if (currentUserService.CurrentUserId is not { } updatedBy)
            return Unauthorized();

        var config = await fineConfigService.UpdateAsync(request, updatedBy, ct);
        return Ok(config);
    }
}
