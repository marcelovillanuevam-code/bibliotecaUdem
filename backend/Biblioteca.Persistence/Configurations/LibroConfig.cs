using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class LibroConfig : IEntityTypeConfiguration<Libro>
{
    public void Configure(EntityTypeBuilder<Libro> builder)
    {
        builder.ToTable("libros");

        builder.HasKey(libro => libro.Id);

        builder.Property(libro => libro.Id)
            .HasColumnName("id");

        builder.Property(libro => libro.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(libro => libro.Autor)
            .HasColumnName("autor")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(libro => libro.Isbn)
            .HasColumnName("isbn")
            .HasMaxLength(17)
            .IsRequired();

        builder.Property(libro => libro.Stock)
            .HasColumnName("stock")
            .IsRequired();

        builder.Property(libro => libro.FechaRegistro)
            .HasColumnName("fecha_registro")
            .IsRequired();

        builder.Property(libro => libro.Disponible)
            .HasColumnName("disponible")
            .IsRequired();

        builder.HasIndex(libro => libro.Isbn).IsUnique();
    }
}
