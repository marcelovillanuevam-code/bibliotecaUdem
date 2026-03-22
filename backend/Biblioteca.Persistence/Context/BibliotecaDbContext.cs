using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Context;

public sealed class BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) : DbContext(options)
{
    public DbSet<RegistroAuditoria> AuditLogs => Set<RegistroAuditoria>();
    public DbSet<Autor> Authors => Set<Autor>();
    public DbSet<LibroAutor> BookAuthors => Set<LibroAutor>();
    public DbSet<LibroMateria> BookSubjects => Set<LibroMateria>();
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
}
