using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class FineConfig : IEntityTypeConfiguration<Fine>
{
    public void Configure(EntityTypeBuilder<Fine> builder)
    {
        builder.ToTable("fines");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.ReturnId).HasColumnName("return_id");
        builder.Property(f => f.UserId).HasColumnName("user_id");

        builder.Property(f => f.Reason)
            .HasColumnName("reason")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(f => f.Amount)
            .HasColumnName("amount")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(f => f.DaysLate).HasColumnName("days_late");

        builder.Property(f => f.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(f => f.PaidAt).HasColumnName("paid_at");
        builder.Property(f => f.PaidByUserId).HasColumnName("paid_by_user_id");
        builder.Property(f => f.DeletedAt).HasColumnName("deleted_at");

        // Concurrency token para operaciones de pago/condonación con contención
        builder.HasAnnotation("Npgsql:UseXminAsConcurrencyToken", true);

        builder.HasQueryFilter(f => f.DeletedAt == null);

        builder.HasIndex(f => new { f.UserId, f.Status })
            .HasDatabaseName("idx_fines_user_status");

        builder.HasIndex(f => f.ReturnId)
            .HasDatabaseName("idx_fines_return_id");

        builder.HasOne(f => f.Return)
            .WithOne(r => r.Fine)
            .HasForeignKey<Fine>(f => f.ReturnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK opcional al usuario tesorería que confirmó el pago
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(f => f.PaidByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
