using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class LibroMateriaConfig : IEntityTypeConfiguration<LibroMateria>
{
    public void Configure(EntityTypeBuilder<LibroMateria> builder)
    {
        builder.ToTable("book_subjects");

        builder.HasKey(libroMateria => new { libroMateria.BookId, libroMateria.SubjectId });

        builder.Property(libroMateria => libroMateria.BookId).HasColumnName("book_id");
        builder.Property(libroMateria => libroMateria.SubjectId).HasColumnName("subject_id");

        builder.HasOne(libroMateria => libroMateria.Book)
            .WithMany(libro => libro.Subjects)
            .HasForeignKey(libroMateria => libroMateria.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(libroMateria => libroMateria.Subject)
            .WithMany(materia => materia.Books)
            .HasForeignKey(libroMateria => libroMateria.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(libroMateria => libroMateria.SubjectId).HasDatabaseName("fk_bs_subject");
    }
}
