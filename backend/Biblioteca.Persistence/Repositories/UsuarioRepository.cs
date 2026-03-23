using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class UsuarioRepository(BibliotecaDbContext dbContext) : IUsuarioRepository
{
    private static readonly Estado[] DefaultStatuses =
    [
        new() { Code = "active", Description = "Cuenta activa." },
        new() { Code = "deleted", Description = "Cuenta eliminada logicamente." },
        new() { Code = "inactive", Description = "Cuenta inactiva." },
        new() { Code = "pending_verification", Description = "Cuenta pendiente de verificacion." },
        new() { Code = "suspended", Description = "Cuenta suspendida." }
    ];

    private static readonly (Guid Id, string Code, string DisplayName, string Description)[] DefaultRoles =
    [
        (Guid.Parse("5ba2f895-23c0-11f1-b6ca-1cce51c6125a"), "STUDENT", "Student", "Alumno de la biblioteca."),
        (Guid.Parse("5ba2ff3e-23c0-11f1-b6ca-1cce51c6125a"), "LIBRARIAN", "Librarian", "Bibliotecario responsable de operacion."),
        (Guid.Parse("5ba30106-23c0-11f1-b6ca-1cce51c6125a"), "TEACHER", "Teacher", "Docente con acceso academico."),
        (Guid.Parse("5ba30193-23c0-11f1-b6ca-1cce51c6125a"), "ADMIN", "Admin", "Administrador del sistema.")
    ];

    public async Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(usuario => usuario.Status)
            .Include(usuario => usuario.Profile)
            .Include(usuario => usuario.Contacts)
            .Include(usuario => usuario.Roles)
                .ThenInclude(usuarioRol => usuarioRol.Role)
            .Where(usuario => usuario.DeletedAt == null)
            .OrderBy(usuario => usuario.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(usuario => usuario.Status)
            .Include(usuario => usuario.Profile)
            .Include(usuario => usuario.Contacts)
            .Include(usuario => usuario.Roles)
                .ThenInclude(usuarioRol => usuarioRol.Role)
            .FirstOrDefaultAsync(usuario => usuario.Id == id && usuario.DeletedAt == null, cancellationToken);
    }

    public async Task<Usuario?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Include(usuario => usuario.Status)
            .Include(usuario => usuario.Auth)
            .Include(usuario => usuario.Profile)
            .Include(usuario => usuario.Contacts)
            .Include(usuario => usuario.Roles)
                .ThenInclude(usuarioRol => usuarioRol.Role)
            .FirstOrDefaultAsync(usuario => usuario.Id == id && usuario.DeletedAt == null, cancellationToken);
    }

    public async Task<Usuario?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();

        return await dbContext.Users
            .Include(usuario => usuario.Auth)
            .Include(usuario => usuario.Profile)
            .Include(usuario => usuario.Contacts)
            .Include(usuario => usuario.Roles)
                .ThenInclude(usuarioRol => usuarioRol.Role)
            .FirstOrDefaultAsync(usuario => usuario.UsernameLower == normalizedUsername, cancellationToken);
    }

    public Task<bool> UsernameExistsAsync(
        string username,
        CancellationToken cancellationToken,
        Guid? excludedUserId = null)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();

        return dbContext.Users
            .AnyAsync(
                usuario => usuario.UsernameLower == normalizedUsername &&
                           (excludedUserId == null || usuario.Id != excludedUserId.Value),
                cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken,
        Guid? excludedUserId = null)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return dbContext.UserContacts
            .AnyAsync(
                contacto =>
                    contacto.Type == "email" &&
                    contacto.Value.ToLower() == normalizedEmail &&
                    (excludedUserId == null || contacto.UserId != excludedUserId.Value),
                cancellationToken);
    }

    public async Task EnsureReferenceDataAsync(CancellationToken cancellationToken)
    {
        var hasChanges = false;

        foreach (var status in DefaultStatuses)
        {
            var existingStatus = await dbContext.Statuses.FindAsync([status.Code], cancellationToken);
            if (existingStatus is not null)
            {
                continue;
            }

            dbContext.Statuses.Add(new Estado
            {
                Code = status.Code,
                Description = status.Description
            });

            hasChanges = true;
        }

        foreach (var role in DefaultRoles)
        {
            var existingRole = await dbContext.Roles.FirstOrDefaultAsync(
                existing => existing.Code == role.Code,
                cancellationToken);

            if (existingRole is not null)
            {
                continue;
            }

            dbContext.Roles.Add(new Rol
            {
                Id = role.Id,
                Code = role.Code,
                DisplayName = role.DisplayName,
                Description = role.Description,
                CreatedAt = DateTime.UtcNow
            });

            hasChanges = true;
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public Task<Rol?> GetRoleByCodeAsync(string roleCode, CancellationToken cancellationToken) =>
        dbContext.Roles.FirstOrDefaultAsync(role => role.Code == roleCode, cancellationToken);

    public async Task<Usuario> AddAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(usuario);
        await dbContext.SaveChangesAsync(cancellationToken);
        return usuario;
    }

    public async Task<Usuario> RegisterAsync(
        Usuario usuario,
        AutenticacionUsuario auth,
        PerfilUsuario profile,
        ContactoUsuario emailContact,
        UsuarioRol userRole,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.Users.Add(usuario);
        dbContext.UserAuth.Add(auth);
        dbContext.UserProfiles.Add(profile);
        dbContext.UserContacts.Add(emailContact);
        dbContext.UserRoles.Add(userRole);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetByUsernameAsync(usuario.Username, cancellationToken)
            ?? throw new InvalidOperationException("No se pudo recuperar el usuario recien registrado.");
    }

    public async Task UpdateAuthAsync(AutenticacionUsuario auth, CancellationToken cancellationToken)
    {
        dbContext.UserAuth.Update(auth);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
