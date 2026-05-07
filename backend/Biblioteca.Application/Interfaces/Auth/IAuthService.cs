using Biblioteca.Application.DTOs.Auth;

namespace Biblioteca.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
    Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
