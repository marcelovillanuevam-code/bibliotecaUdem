using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Id)
            .HasColumnName("id");

        builder.Property(usuario => usuario.NombreCompleto)
            .HasColumnName("nombre_completo")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(usuario => usuario.Email)
            .HasColumnName("email")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(usuario => usuario.Matricula)
            .HasColumnName("matricula")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(usuario => usuario.FechaRegistro)
            .HasColumnName("fecha_registro")
            .IsRequired();

        builder.Property(usuario => usuario.Activo)
            .HasColumnName("activo")
            .IsRequired();

        builder.HasIndex(usuario => usuario.Email).IsUnique();
        builder.HasIndex(usuario => usuario.Matricula).IsUnique();
    }
}
