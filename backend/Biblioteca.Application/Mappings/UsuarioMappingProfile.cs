using AutoMapper;
using Biblioteca.Application.DTOs.Auth;
using Biblioteca.Application.DTOs.Usuarios;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Mappings;

public sealed class UsuarioMappingProfile : Profile
{
    public UsuarioMappingProfile()
    {
        CreateMap<Usuario, UsuarioDto>()
            .ConstructUsing(src => new UsuarioDto(
                src.Id,
                src.Username,
                ResolveFirstName(src),
                ResolveLastName(src),
                ResolveDisplayName(src),
                ResolveEmail(src),
                ResolveUniversityId(src),
                ResolveDocumentType(src),
                ResolveDocumentNumber(src),
                ResolveRoleCode(src),
                ResolveRoleLabel(src),
                src.StatusCode,
                ResolveStatusLabel(src),
                src.PreferredLocale,
                src.MetadataJson,
                src.CreatedAt,
                src.UpdatedAt,
                src.DeletedAt));

        CreateMap<Usuario, AuthenticatedUserDto>()
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src =>
                src.Profile != null
                    ? string.IsNullOrWhiteSpace(src.Profile.DisplayName)
                        ? $"{src.Profile.FirstName} {src.Profile.LastName}".Trim()
                        : src.Profile.DisplayName
                    : src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src =>
                src.Contacts
                    .OrderByDescending(contact => contact.IsPrimary)
                    .FirstOrDefault(contact => contact.Type == "email") != null
                        ? src.Contacts
                            .OrderByDescending(contact => contact.IsPrimary)
                            .FirstOrDefault(contact => contact.Type == "email")!.Value
                        : string.Empty))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.Roles
                    .Where(role => role.Role != null)
                    .Select(role => role.Role!.Code)
                    .ToArray()));
    }

    private static string ResolveDisplayName(Usuario usuario)
    {
        if (usuario.Profile is null)
        {
            return usuario.Username;
        }

        if (!string.IsNullOrWhiteSpace(usuario.Profile.DisplayName))
        {
            return usuario.Profile.DisplayName.Trim();
        }

        return $"{usuario.Profile.FirstName} {usuario.Profile.LastName}".Trim();
    }

    private static string ResolveFirstName(Usuario usuario) =>
        usuario.Profile is null ? string.Empty : usuario.Profile.FirstName;

    private static string ResolveLastName(Usuario usuario) =>
        usuario.Profile is null ? string.Empty : usuario.Profile.LastName;

    private static string ResolveUniversityId(Usuario usuario) =>
        usuario.Profile is null || string.IsNullOrWhiteSpace(usuario.Profile.DocumentNumber)
            ? string.Empty
            : usuario.Profile.DocumentNumber;

    private static string? ResolveDocumentType(Usuario usuario) =>
        usuario.Profile is null || string.IsNullOrWhiteSpace(usuario.Profile.DocumentType)
            ? null
            : usuario.Profile.DocumentType;

    private static string? ResolveDocumentNumber(Usuario usuario) =>
        usuario.Profile is null || string.IsNullOrWhiteSpace(usuario.Profile.DocumentNumber)
            ? null
            : usuario.Profile.DocumentNumber;

    private static string ResolveEmail(Usuario usuario) =>
        usuario.Contacts
            .OrderByDescending(contact => contact.IsPrimary)
            .FirstOrDefault(contact => contact.Type == "email")?.Value
        ?? string.Empty;

    private static string ResolveRoleCode(Usuario usuario) =>
        usuario.Roles
            .OrderByDescending(role => role.AssignedAt)
            .FirstOrDefault(role => role.Role is not null)?.Role?.Code
        ?? string.Empty;

    private static string ResolveRoleLabel(Usuario usuario)
    {
        var role = usuario.Roles
            .OrderByDescending(userRole => userRole.AssignedAt)
            .FirstOrDefault(userRole => userRole.Role is not null)?.Role;

        return role?.Description ?? role?.DisplayName ?? role?.Code ?? string.Empty;
    }

    private static string ResolveStatusLabel(Usuario usuario) =>
        string.IsNullOrWhiteSpace(usuario.Status?.Description)
            ? usuario.StatusCode
            : usuario.Status.Description;
}
