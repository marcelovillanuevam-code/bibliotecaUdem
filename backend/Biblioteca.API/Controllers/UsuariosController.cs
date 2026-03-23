using Biblioteca.Application.DTOs.Usuarios;
using Biblioteca.Application.Features.Usuarios;
using Biblioteca.Application.Interfaces.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
[Route(UsuariosFeature.Route)]
public sealed class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<UsuarioDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var usuarios = await usuarioService.GetAllAsync(cancellationToken);
        return Ok(usuarios);
    }

    [HttpGet("{id:guid}", Name = "GetUsuarioById")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.GetByIdAsync(id, cancellationToken);

        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(usuario);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioDto>> CreateAsync(
        [FromBody] CreateUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute("GetUsuarioById", new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.UpdateAsync(id, request, cancellationToken);
        return Ok(usuario);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await usuarioService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
