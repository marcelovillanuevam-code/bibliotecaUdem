namespace Biblioteca.Application.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// Dominio de correo permitido para el registro. Configurable en appsettings Auth:AllowedEmailDomain.
    /// </summary>
    public string AllowedEmailDomain { get; init; } = "udem.edu";
}
