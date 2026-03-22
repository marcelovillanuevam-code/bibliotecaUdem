using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Configurations;
using Biblioteca.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Context;

public sealed class BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Libro> Libros => Set<Libro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsuarioConfig());
        modelBuilder.ApplyConfiguration(new LibroConfig());

        modelBuilder.Entity<Usuario>().HasData(SeedData.Usuarios);
        modelBuilder.Entity<Libro>().HasData(SeedData.Libros);

        base.OnModelCreating(modelBuilder);
    }
}
