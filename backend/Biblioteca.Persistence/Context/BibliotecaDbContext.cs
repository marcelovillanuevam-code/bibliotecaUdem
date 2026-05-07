using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Context;

public sealed class BibliotecaDbContext(
    DbContextOptions<BibliotecaDbContext> options,
    ICurrentUserService currentUserService) : DbContext(options)
{
    public DbSet<RegistroAuditoria> AuditLogs => Set<RegistroAuditoria>();
    public DbSet<Autor> Authors => Set<Autor>();
    public DbSet<LibroAutor> BookAuthors => Set<LibroAutor>();
    public DbSet<LibroMateria> BookSubjects => Set<LibroMateria>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<Libro> Books => Set<Libro>();
    public DbSet<Ubicacion> Locations => Set<Ubicacion>();
    public DbSet<TokenRestablecimientoContrasena> PasswordResetTokens => Set<TokenRestablecimientoContrasena>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Sesion> Sessions => Set<Sesion>();
    public DbSet<Estado> Statuses => Set<Estado>();
    public DbSet<Materia> Subjects => Set<Materia>();
    public DbSet<AutenticacionUsuario> UserAuth => Set<AutenticacionUsuario>();
    public DbSet<ContactoUsuario> UserContacts => Set<ContactoUsuario>();
    public DbSet<PerfilUsuario> UserProfiles => Set<PerfilUsuario>();
    public DbSet<UsuarioRol> UserRoles => Set<UsuarioRol>();
    public DbSet<Usuario> Users => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BibliotecaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = CreateAuditEntries();
        foreach (var entry in auditEntries)
            AuditLogs.Add(entry);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private List<RegistroAuditoria> CreateAuditEntries()
    {
        var userId = currentUserService.CurrentUserId;
        if (userId is null) return [];

        var now = DateTime.UtcNow;
        var entries = new List<RegistroAuditoria>();

        foreach (var entityEntry in ChangeTracker.Entries())
        {
            if (entityEntry.Entity is RegistroAuditoria) continue;

            var action = entityEntry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => null
            };

            if (action is null) continue;

            var (tableName, recordId) = GetAuditInfo(entityEntry.Entity);
            if (tableName is null) continue;

            entries.Add(new RegistroAuditoria
            {
                Id = Guid.NewGuid(),
                TableName = tableName,
                RecordId = recordId,
                Action = action,
                PerformedBy = userId,
                PerformedAt = now
            });
        }

        return entries;
    }

    private static (string? TableName, Guid? RecordId) GetAuditInfo(object entity) =>
        entity switch
        {
            Usuario u => ("users", u.Id),
            Libro l => ("books", l.Id),
            BookCopy bc => ("book_copies", bc.Id),
            _ => (null, null)
        };
}
