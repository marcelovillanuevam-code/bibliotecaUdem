using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class LoanConfig : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.UserId).HasColumnName("user_id");
        builder.Property(l => l.BookCopyId).HasColumnName("book_copy_id");
        builder.Property(l => l.LoanedAt).HasColumnName("loaned_at").IsRequired();
        builder.Property(l => l.DueAt).HasColumnName("due_at").IsRequired();
        builder.Property(l => l.ReturnedAt).HasColumnName("returned_at");

        builder.Property(l => l.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(l => l.RenewalCount)
            .HasColumnName("renewal_count")
            .HasDefaultValue(0);

        builder.Property(l => l.IssuedByUserId).HasColumnName("issued_by_user_id");
        builder.Property(l => l.DeletedAt).HasColumnName("deleted_at");

        // Concurrency token via columna de sistema xmin de PostgreSQL
        builder.HasAnnotation("Npgsql:UseXminAsConcurrencyToken", true);

        builder.HasQueryFilter(l => l.DeletedAt == null);

        builder.HasIndex(l => new { l.UserId, l.Status })
            .HasDatabaseName("loans_user_status_idx");

        builder.HasIndex(l => new { l.BookCopyId, l.Status })
            .HasDatabaseName("loans_book_copy_status_idx");

        // Índice único parcial: una copia no puede tener dos préstamos activos simultáneos
        builder.HasIndex(l => l.BookCopyId)
            .IsUnique()
            .HasFilter("status = 'ACTIVE'")
            .HasDatabaseName("loans_active_copy_unique");

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.BookCopy)
            .WithMany()
            .HasForeignKey(l => l.BookCopyId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK al bibliotecario que registró — sin nav prop, sin cascada
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(l => l.IssuedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Renewals)
            .WithOne(r => r.Loan)
            .HasForeignKey(r => r.LoanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
