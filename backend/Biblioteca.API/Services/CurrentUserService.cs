using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Biblioteca.Application.Interfaces.Common;

namespace Biblioteca.API.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? CurrentUserId
    {
        get
        {
            // .NET 8+ JwtBearerHandler mapea "sub" a ClaimTypes.NameIdentifier por defecto.
            // Se busca primero el nombre mapeado y luego el nombre original como fallback.
            var value = httpContextAccessor.HttpContext?.User
                            .FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? httpContextAccessor.HttpContext?.User
                            .FindFirstValue(JwtRegisteredClaimNames.Sub);

            return value is not null && Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
