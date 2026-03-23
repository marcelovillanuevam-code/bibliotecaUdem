namespace Biblioteca.Application.DTOs.Auth;

public sealed record JwtTokenResult(
    string AccessToken,
    DateTime ExpiresAtUtc);
