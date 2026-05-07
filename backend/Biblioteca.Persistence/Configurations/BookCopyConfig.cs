using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class BookCopyConfig : IEntityTypeConfiguration<BookCopy>
{
    public void Configure(EntityTypeBuilder<BookCopy> builder)
    {
        builder.ToTable("book_copies");

        builder.HasKey(bc => bc.Id);

        builder.Property(bc => bc.Id).HasColumnName("id");
        builder.Property(bc => bc.BookId).HasColumnName("book_id");

        builder.Property(bc => bc.Barcode)
            .HasColumnName("barcode")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(bc => bc.LocationId).HasColumnName("location_id");

        builder.Property(bc => bc.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(bc => bc.Condition)
            .HasColumnName("condition")
            .HasMaxLength(50);

        builder.Property(bc => bc.AcquiredAt)
            .HasColumnName("acquired_at")
            .IsRequired();

        builder.Property(bc => bc.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(bc => bc.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(bc => bc.DeletedAt).HasColumnName("deleted_at");

        // UseXminAsConcurrencyToken vía anotación directa (misma semántica que la extension method de Npgsql)
        builder.HasAnnotation("Npgsql:UseXminAsConcurrencyToken", true);

        builder.HasIndex(bc => bc.Barcode).IsUnique().HasDatabaseName("uq_book_copies_barcode");
        builder.HasIndex(bc => bc.BookId).HasDatabaseName("idx_book_copies_book_id");

        builder.HasQueryFilter(bc => bc.DeletedAt == null);

        builder.HasOne(bc => bc.Book)
            .WithMany(l => l.Copies)
            .HasForeignKey(bc => bc.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bc => bc.Location)
            .WithMany()
            .HasForeignKey(bc => bc.LocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
