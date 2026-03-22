using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class LibroConfig : IEntityTypeConfiguration<Libro>
{
    public void Configure(EntityTypeBuilder<Libro> builder)
    {
        builder.ToTable("books");

        builder.HasKey(libro => libro.Id);

        builder.Property(libro => libro.Id)
            .HasColumnName("id");

        builder.Property(libro => libro.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(libro => libro.Subtitle)
            .HasColumnName("subtitle")
            .HasMaxLength(500);

        builder.Property(libro => libro.Isbn)
            .HasColumnName("isbn")
            .HasMaxLength(50);

        builder.Property(libro => libro.Publisher)
            .HasColumnName("publisher")
            .HasMaxLength(255);

        builder.Property(libro => libro.PublicationYear)
            .HasColumnName("publication_year");

        builder.Property(libro => libro.Edition)
            .HasColumnName("edition")
            .HasMaxLength(100);

        builder.Property(libro => libro.Language)
            .HasColumnName("language")
            .HasMaxLength(50)
            .HasDefaultValue("es")
            .IsRequired();

        builder.Property(libro => libro.SummaryJson)
            .HasColumnName("summary")
            .HasColumnType("jsonb");

        builder.Property(libro => libro.MetadataJson)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(libro => libro.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(libro => libro.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(libro => libro.Isbn).IsUnique();
        builder.HasIndex(libro => libro.Title).HasDatabaseName("idx_books_title");
        builder.HasIndex(libro => libro.Isbn).HasDatabaseName("idx_books_isbn");
    }
}
