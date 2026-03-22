using Biblioteca.Application.DTOs.Usuarios;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Application.Mappings;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Services.Usuarios;

public sealed class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IDateTimeProvider dateTimeProvider) : IUsuarioService
{
    public async Task<IReadOnlyCollection<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var usuarios = await usuarioRepository.GetAllAsync(cancellationToken);
        return usuarios.Select(usuario => usuario.ToDto()).ToArray();
    }

    public async Task<UsuarioDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByIdAsync(id, cancellationToken);
        return usuario?.ToDto();
    }

    public async Task<UsuarioDto> CreateAsync(CreateUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            NombreCompleto = request.NombreCompleto.Trim(),
            Email = request.Email.Trim(),
            Matricula = request.Matricula.Trim(),
            FechaRegistro = dateTimeProvider.UtcNow,
            Activo = true
        };

        var createdUsuario = await usuarioRepository.AddAsync(usuario, cancellationToken);

        return createdUsuario.ToDto();
    }
}
