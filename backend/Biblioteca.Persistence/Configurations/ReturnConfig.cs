using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class ReturnConfig : IEntityTypeConfiguration<Return>
{
    public void Configure(EntityTypeBuilder<Return> builder)
    {
        builder.ToTable("returns");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.LoanId).HasColumnName("loan_id");
        builder.Property(r => r.ReturnedAt).HasColumnName("returned_at").IsRequired();

        builder.Property(r => r.Condition)
            .HasColumnName("condition")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.InspectionNotes)
            .HasColumnName("inspection_notes")
            .HasMaxLength(1000);

        builder.Property(r => r.ReceivedByUserId).HasColumnName("received_by_user_id");
        builder.Property(r => r.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(r => r.DeletedAt == null);

        // Un préstamo se devuelve exactamente una vez
        builder.HasIndex(r => r.LoanId)
            .IsUnique()
            .HasDatabaseName("uq_returns_loan_id");

        // No borrar el préstamo si tiene devolución registrada
        builder.HasOne(r => r.Loan)
            .WithMany()
            .HasForeignKey(r => r.LoanId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK al bibliotecario que recibió — sin nav prop en Usuario
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(r => r.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
