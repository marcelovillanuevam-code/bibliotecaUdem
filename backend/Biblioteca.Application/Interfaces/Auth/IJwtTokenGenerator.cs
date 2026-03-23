using Biblioteca.Application.DTOs.Auth;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Auth;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(Usuario usuario);
}
