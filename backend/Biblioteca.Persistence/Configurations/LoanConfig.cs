using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class LoanConfig : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans");

        builder.HasKey(loan => loan.Id);

        builder.Property(loan => loan.Id).HasColumnName("id");
        builder.Property(loan => loan.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(loan => loan.BookCopyId).HasColumnName("book_copy_id").IsRequired();
        builder.Property(loan => loan.LoanedAt).HasColumnName("loaned_at").IsRequired();
        builder.Property(loan => loan.DueAt).HasColumnName("due_at").IsRequired();
        builder.Property(loan => loan.ReturnedAt).HasColumnName("returned_at");

        builder.Property(loan => loan.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(loan => loan.RenewalCount)
            .HasColumnName("renewal_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(loan => loan.IssuedByUserId)
            .HasColumnName("issued_by_user_id")
            .IsRequired();

        builder.Property(loan => loan.DeletedAt).HasColumnName("deleted_at");

        builder.Property(loan => loan.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(loan => loan.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Concurrency token vía xmin (mismo patrón que BookCopy).
        builder.HasAnnotation("Npgsql:UseXminAsConcurrencyToken", true);

        builder.HasIndex(loan => new { loan.UserId, loan.Status })
            .HasDatabaseName("idx_loans_user_status");

        builder.HasIndex(loan => new { loan.BookCopyId, loan.Status })
            .HasDatabaseName("idx_loans_book_copy_status");

        // Único parcial: una copia no puede tener dos préstamos ACTIVE simultáneos.
        builder.HasIndex(loan => loan.BookCopyId)
            .IsUnique()
            .HasFilter("status = 'ACTIVE'")
            .HasDatabaseName("uq_loans_active_copy");

        builder.HasQueryFilter(loan => loan.DeletedAt == null);

        builder.HasOne(loan => loan.User)
            .WithMany()
            .HasForeignKey(loan => loan.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(loan => loan.BookCopy)
            .WithMany()
            .HasForeignKey(loan => loan.BookCopyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(loan => loan.IssuedByUser)
            .WithMany()
            .HasForeignKey(loan => loan.IssuedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(loan => loan.Renewals)
            .WithOne(renewal => renewal.Loan)
            .HasForeignKey(renewal => renewal.LoanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
