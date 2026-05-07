using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Biblioteca.Application.DTOs.Auth;
using Biblioteca.Application.Interfaces.Auth;
using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Biblioteca.Infrastructure.Services;

public sealed class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions) : IJwtTokenGenerator
{
    public JwtTokenResult GenerateToken(Usuario usuario)
    {
        var options = jwtOptions.Value;
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(options.ExpiresInMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, usuario.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("preferred_locale", usuario.PreferredLocale)
        };

        claims.AddRange(
            usuario.Roles
                .Where(userRole => userRole.Role is not null)
                .Select(userRole => new Claim(ClaimTypes.Role, userRole.Role!.Code)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
