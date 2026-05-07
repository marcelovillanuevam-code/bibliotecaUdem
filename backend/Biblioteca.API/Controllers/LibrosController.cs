using Biblioteca.API.Extensions;
using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Features.Libros;
using Biblioteca.Application.Interfaces.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route(LibrosFeature.Route)]
public sealed class LibrosController(ILibroService libroService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(IReadOnlyCollection<LibroDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<LibroDto>>> GetAllAsync(
        [FromQuery] GetLibrosRequest request,
        CancellationToken cancellationToken)
    {
        var libros = await libroService.GetAllAsync(request, cancellationToken);
        return Ok(libros);
    }

    [HttpGet("{id:guid}", Name = "GetLibroById")]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(LibroFichaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LibroFichaDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var libro = await libroService.GetByIdAsync(id, cancellationToken);

        if (libro is null)
        {
            return NotFound();
        }

        return Ok(libro);
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(LibroFichaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LibroFichaDto>> CreateAsync(
        [FromBody] CreateLibroRequest request,
        CancellationToken cancellationToken)
    {
        var libro = await libroService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute("GetLibroById", new { id = libro.Id }, libro);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(LibroFichaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LibroFichaDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateLibroRequest request,
        CancellationToken cancellationToken)
    {
        var libro = await libroService.UpdateAsync(id, request, cancellationToken);
        return Ok(libro);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await libroService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
