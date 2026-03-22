using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class LibroAutorConfig : IEntityTypeConfiguration<LibroAutor>
{
    public void Configure(EntityTypeBuilder<LibroAutor> builder)
    {
        builder.ToTable("book_authors");

        builder.HasKey(libroAutor => new { libroAutor.BookId, libroAutor.AuthorId });

        builder.Property(libroAutor => libroAutor.BookId).HasColumnName("book_id");
        builder.Property(libroAutor => libroAutor.AuthorId).HasColumnName("author_id");
        builder.Property(libroAutor => libroAutor.Contribution).HasColumnName("contribution").HasMaxLength(100);

        builder.HasOne(libroAutor => libroAutor.Book)
            .WithMany(libro => libro.Authors)
            .HasForeignKey(libroAutor => libroAutor.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(libroAutor => libroAutor.Author)
            .WithMany(autor => autor.Books)
            .HasForeignKey(libroAutor => libroAutor.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(libroAutor => libroAutor.AuthorId).HasDatabaseName("fk_ba_author");
    }
}
