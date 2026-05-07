namespace Biblioteca.API.Extensions;

// Nombres de policies centralizados. Si se agregan roles nuevos, actualizar aquí.
internal static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string AdminOrLibrarian = "AdminOrLibrarian";
    public const string Authenticated = "Authenticated";
}
