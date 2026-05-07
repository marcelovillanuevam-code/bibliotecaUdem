using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Biblioteca.Application.DTOs.Usuarios;
using Biblioteca.Application.Exceptions;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Biblioteca.Application.Services.Usuarios;

public sealed class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher<Usuario> passwordHasher,
    IDateTimeProvider dateTimeProvider,
    IMapper mapper) : IUsuarioService
{
    private static readonly HashSet<string> AllowedStatusCodes =
    [
        "active",
        "inactive",
        "pending_verification",
        "suspended"
    ];

    public async Task<IReadOnlyCollection<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var usuarios = await usuarioRepository.GetAllAsync(cancellationToken);
        return mapper.Map<IReadOnlyCollection<UsuarioDto>>(usuarios);
    }

    public async Task<UsuarioDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByIdAsync(id, cancellationToken);
        return usuario is null ? null : mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto> CreateAsync(CreateUsuarioRequest request, CancellationToken cancellationToken)
    {
        await usuarioRepository.EnsureReferenceDataAsync(cancellationToken);

        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var statusCode = NormalizeStatusCode(request.StatusCode);
        var preferredLocale = NormalizePreferredLocale(request.PreferredLocale);

        if (await usuarioRepository.UsernameExistsAsync(username, cancellationToken))
        {
            throw new ConflictException("El nombre de usuario ya esta registrado.");
        }

        if (await usuarioRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("El correo electronico ya esta registrado.");
        }

        var role = await ResolveRoleAsync(request.RoleCode, cancellationToken);
        var now = dateTimeProvider.UtcNow;

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Username = username,
            StatusCode = statusCode,
            PreferredLocale = preferredLocale,
            MetadataJson = request.MetadataJson,
            CreatedAt = now,
            UpdatedAt = now
        };

        var auth = new AutenticacionUsuario
        {
            UserId = usuario.Id,
            PasswordHash = passwordHasher.HashPassword(usuario, request.Password),
            PasswordChangedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        var profile = new PerfilUsuario
        {
            UserId = usuario.Id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DisplayName = BuildDisplayName(request.DisplayName, request.FirstName, request.LastName),
            DocumentType = NormalizeOptional(request.DocumentType),
            DocumentNumber = NormalizeOptional(request.DocumentNumber),
            CreatedAt = now,
            UpdatedAt = now
        };

        var emailContact = new ContactoUsuario
        {
            Id = Guid.NewGuid(),
            UserId = usuario.Id,
            Type = "email",
            Value = email,
            IsPrimary = true,
            IsVerified = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        var userRole = new UsuarioRol
        {
            UserId = usuario.Id,
            RoleId = role.Id,
            AssignedAt = now,
            Role = role
        };

        usuario.Auth = auth;
        usuario.Profile = profile;
        usuario.Contacts.Add(emailContact);
        usuario.Roles.Add(userRole);

        var createdUser = await usuarioRepository.RegisterAsync(
            usuario,
            auth,
            profile,
            emailContact,
            userRole,
            cancellationToken);

        return mapper.Map<UsuarioDto>(createdUser);
    }

    public async Task<UsuarioDto> UpdateAsync(Guid id, UpdateUsuarioRequest request, CancellationToken cancellationToken)
    {
        await usuarioRepository.EnsureReferenceDataAsync(cancellationToken);

        var usuario = await usuarioRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("No se encontro el usuario solicitado.");

        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var statusCode = NormalizeStatusCode(request.StatusCode);
        var preferredLocale = NormalizePreferredLocale(request.PreferredLocale);

        if (await usuarioRepository.UsernameExistsAsync(username, cancellationToken, id))
        {
            throw new ConflictException("El nombre de usuario ya esta registrado.");
        }

        if (await usuarioRepository.EmailExistsAsync(email, cancellationToken, id))
        {
            throw new ConflictException("El correo electronico ya esta registrado.");
        }

        var role = await ResolveRoleAsync(request.RoleCode, cancellationToken);
        var now = dateTimeProvider.UtcNow;

        usuario.Username = username;
        usuario.StatusCode = statusCode;
        usuario.PreferredLocale = preferredLocale;
        usuario.MetadataJson = request.MetadataJson;
        usuario.UpdatedAt = now;
        usuario.DeletedAt = statusCode == "deleted" ? now : null;

        var profile = usuario.Profile ?? new PerfilUsuario
        {
            UserId = usuario.Id,
            CreatedAt = now
        };

        profile.FirstName = request.FirstName.Trim();
        profile.LastName = request.LastName.Trim();
        profile.DisplayName = BuildDisplayName(request.DisplayName, request.FirstName, request.LastName);
        profile.DocumentType = NormalizeOptional(request.DocumentType);
        profile.DocumentNumber = NormalizeOptional(request.DocumentNumber);
        profile.UpdatedAt = now;
        usuario.Profile = profile;

        var emailContact = usuario.Contacts
            .OrderByDescending(contact => contact.IsPrimary)
            .FirstOrDefault(contact => contact.Type == "email");

        if (emailContact is null)
        {
            emailContact = new ContactoUsuario
            {
                Id = Guid.NewGuid(),
                UserId = usuario.Id,
                Type = "email",
                CreatedAt = now
            };

            usuario.Contacts.Add(emailContact);
        }

        emailContact.Value = email;
        emailContact.IsPrimary = true;
        emailContact.UpdatedAt = now;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var auth = usuario.Auth ?? new AutenticacionUsuario
            {
                UserId = usuario.Id,
                CreatedAt = now
            };

            auth.PasswordHash = passwordHasher.HashPassword(usuario, request.Password.Trim());
            auth.PasswordChangedAt = now;
            auth.UpdatedAt = now;
            usuario.Auth = auth;
        }

        var assignedRole = usuario.Roles.OrderByDescending(userRole => userRole.AssignedAt).FirstOrDefault();
        if (assignedRole is null)
        {
            usuario.Roles.Add(new UsuarioRol
            {
                UserId = usuario.Id,
                RoleId = role.Id,
                Role = role,
                AssignedAt = now
            });
        }
        else
        {
            assignedRole.RoleId = role.Id;
            assignedRole.Role = role;
            assignedRole.AssignedAt = now;
        }

        await usuarioRepository.SaveChangesAsync(cancellationToken);

        var updatedUser = await usuarioRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("No se pudo recuperar el usuario actualizado.");

        return mapper.Map<UsuarioDto>(updatedUser);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("No se encontro el usuario solicitado.");

        var now = dateTimeProvider.UtcNow;

        usuario.StatusCode = "deleted";
        usuario.DeletedAt = now;
        usuario.UpdatedAt = now;

        if (usuario.Auth is not null)
        {
            usuario.Auth.LockedUntil = now.AddYears(100);
            usuario.Auth.UpdatedAt = now;
        }

        await usuarioRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Rol> ResolveRoleAsync(string roleCode, CancellationToken cancellationToken)
    {
        var normalizedRoleCode = NormalizeRequiredValue(roleCode, "El rol es obligatorio.").ToUpperInvariant();

        return await usuarioRepository.GetRoleByCodeAsync(normalizedRoleCode, cancellationToken)
            ?? throw new ValidationException($"No existe el rol '{normalizedRoleCode}'.");
    }

    private static string NormalizeStatusCode(string statusCode)
    {
        var normalizedStatusCode = NormalizeRequiredValue(statusCode, "El estatus es obligatorio.").ToLowerInvariant();

        if (!AllowedStatusCodes.Contains(normalizedStatusCode))
        {
            throw new ValidationException($"El estatus '{normalizedStatusCode}' no es valido.");
        }

        return normalizedStatusCode;
    }

    private static string NormalizePreferredLocale(string preferredLocale) =>
        string.IsNullOrWhiteSpace(preferredLocale) ? "es_MX" : preferredLocale.Trim();

    private static string BuildDisplayName(string? displayName, string firstName, string lastName)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName.Trim();
        }

        return $"{firstName.Trim()} {lastName.Trim()}".Trim();
    }

    private static string NormalizeRequiredValue(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(message);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
