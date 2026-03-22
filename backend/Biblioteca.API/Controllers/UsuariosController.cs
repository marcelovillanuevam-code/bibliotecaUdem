using Biblioteca.Application.DTOs.Usuarios;
using Biblioteca.Application.Features.Usuarios;
using Biblioteca.Application.Interfaces.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Route(UsuariosFeature.Route)]
public sealed class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<UsuarioDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var usuarios = await usuarioService.GetAllAsync(cancellationToken);
        return Ok(usuarios);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioDto>> CreateAsync(
        [FromBody] CreateUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = usuario.Id }, usuario);
    }
}
