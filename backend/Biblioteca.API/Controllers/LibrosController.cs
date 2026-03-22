using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Features.Libros;
using Biblioteca.Application.Interfaces.Libros;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Route(LibrosFeature.Route)]
public sealed class LibrosController(ILibroService libroService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<LibroDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<LibroDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var libros = await libroService.GetAllAsync(cancellationToken);
        return Ok(libros);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LibroDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LibroDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var libro = await libroService.GetByIdAsync(id, cancellationToken);

        if (libro is null)
        {
            return NotFound();
        }

        return Ok(libro);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LibroDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LibroDto>> CreateAsync(
        [FromBody] CreateLibroRequest request,
        CancellationToken cancellationToken)
    {
        var libro = await libroService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = libro.Id }, libro);
    }
}
