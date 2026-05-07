using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class LoanRenewalConfig : IEntityTypeConfiguration<LoanRenewal>
{
    public void Configure(EntityTypeBuilder<LoanRenewal> builder)
    {
        builder.ToTable("loan_renewals");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.LoanId).HasColumnName("loan_id");
        builder.Property(r => r.RenewedAt).HasColumnName("renewed_at").IsRequired();
        builder.Property(r => r.PreviousDueAt).HasColumnName("previous_due_at").IsRequired();
        builder.Property(r => r.NewDueAt).HasColumnName("new_due_at").IsRequired();
        builder.Property(r => r.RenewedByUserId).HasColumnName("renewed_by_user_id");

        builder.HasIndex(r => r.LoanId).HasDatabaseName("idx_loan_renewals_loan_id");
    }
}
