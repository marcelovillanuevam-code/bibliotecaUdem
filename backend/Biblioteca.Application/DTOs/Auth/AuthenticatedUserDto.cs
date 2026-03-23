namespace Biblioteca.Application.DTOs.Auth;

public sealed class AuthenticatedUserDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string StatusCode { get; init; } = string.Empty;
    public string PreferredLocale { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; init; } = [];
}
