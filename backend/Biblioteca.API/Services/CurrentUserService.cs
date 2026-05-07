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
            var value = httpContextAccessor.HttpContext?.User
                .FindFirstValue(JwtRegisteredClaimNames.Sub);

            return value is not null && Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
