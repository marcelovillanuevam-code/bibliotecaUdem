using Biblioteca.API.Extensions;
using Biblioteca.Application.DTOs.Dashboard;
using Biblioteca.Application.Features.Dashboard;
using Biblioteca.Application.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route(DashboardFeature.Route)]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("kpis")]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(DashboardKpisDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardKpisDto>> GetKpisAsync(CancellationToken ct)
    {
        var result = await dashboardService.GetKpisAsync(ct);
        return Ok(result);
    }
}
