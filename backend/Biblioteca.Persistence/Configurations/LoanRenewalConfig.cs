using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class LoanRenewalConfig : IEntityTypeConfiguration<LoanRenewal>
{
    public void Configure(EntityTypeBuilder<LoanRenewal> builder)
    {
        builder.ToTable("loan_renewals");

        builder.HasKey(renewal => renewal.Id);

        builder.Property(renewal => renewal.Id).HasColumnName("id");
        builder.Property(renewal => renewal.LoanId).HasColumnName("loan_id").IsRequired();
        builder.Property(renewal => renewal.RenewedAt).HasColumnName("renewed_at").IsRequired();
        builder.Property(renewal => renewal.PreviousDueAt).HasColumnName("previous_due_at").IsRequired();
        builder.Property(renewal => renewal.NewDueAt).HasColumnName("new_due_at").IsRequired();
        builder.Property(renewal => renewal.RenewedByUserId)
            .HasColumnName("renewed_by_user_id")
            .IsRequired();

        builder.HasIndex(renewal => renewal.LoanId).HasDatabaseName("idx_loan_renewals_loan_id");

        builder.HasOne(renewal => renewal.RenewedByUser)
            .WithMany()
            .HasForeignKey(renewal => renewal.RenewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Filtro simétrico al HasQueryFilter de Loan para evitar warning EF y orphans en queries directas.
        builder.HasQueryFilter(renewal => renewal.Loan!.DeletedAt == null);

        // Relación con Loan ya configurada en LoanConfig (HasMany/WithOne).
    }
}
