using Biblioteca.API.Extensions;
using Biblioteca.Application.DTOs.Reportes;
using Biblioteca.Application.Features;
using Biblioteca.Application.Interfaces.Reportes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route(ReportesFeature.Route)]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public sealed class ReportesController(IReportesService reportesService) : ControllerBase
{
    [HttpGet("multas-recaudadas")]
    [ProducesResponseType(typeof(MultasRecaudadasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MultasRecaudadasDto>> GetMultasRecaudadas(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        if (to < from) return BadRequest("El parámetro 'to' debe ser mayor o igual a 'from'.");
        var result = await reportesService.GetMultasRecaudadasAsync(from, to, ct);
        return Ok(result);
    }

    [HttpGet("multas-pendientes")]
    [ProducesResponseType(typeof(IReadOnlyCollection<DeudorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<DeudorDto>>> GetMultasPendientes(CancellationToken ct)
    {
        var result = await reportesService.GetMultasPendientesAsync(ct);
        return Ok(result);
    }

    [HttpGet("devoluciones-tardias")]
    [ProducesResponseType(typeof(DevolucionesTardiasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DevolucionesTardiasDto>> GetDevolucionesTardias(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        if (to < from) return BadRequest("El parámetro 'to' debe ser mayor o igual a 'from'.");
        var result = await reportesService.GetDevolucionesTardiasAsync(from, to, ct);
        return Ok(result);
    }

    [HttpGet("condonaciones")]
    [ProducesResponseType(typeof(IReadOnlyCollection<CondonacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<CondonacionDto>>> GetCondonaciones(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        if (to < from) return BadRequest("El parámetro 'to' debe ser mayor o igual a 'from'.");
        var result = await reportesService.GetCondonacionesAsync(from, to, ct);
        return Ok(result);
    }
}
