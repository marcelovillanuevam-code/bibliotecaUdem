using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class ReservationConfig : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.UserId).HasColumnName("user_id");
        builder.Property(r => r.BookId).HasColumnName("book_id");
        builder.Property(r => r.QueuePosition).HasColumnName("queue_position");

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(r => r.ReadyAt).HasColumnName("ready_at");
        builder.Property(r => r.ExpiresAt).HasColumnName("expires_at");
        builder.Property(r => r.FulfilledAt).HasColumnName("fulfilled_at");
        builder.Property(r => r.FulfilledByLoanId).HasColumnName("fulfilled_by_loan_id");
        builder.Property(r => r.DeletedAt).HasColumnName("deleted_at");

        // Concurrency token para operaciones con contención (expiración, promoción de cola)
        builder.HasAnnotation("Npgsql:UseXminAsConcurrencyToken", true);

        builder.HasQueryFilter(r => r.DeletedAt == null);

        builder.HasIndex(r => new { r.BookId, r.Status, r.QueuePosition })
            .HasDatabaseName("reservations_book_status_pos_idx");

        builder.HasIndex(r => new { r.UserId, r.Status })
            .HasDatabaseName("reservations_user_status_idx");

        // Un usuario no puede tener dos reservas activas del mismo título
        builder.HasIndex(r => new { r.UserId, r.BookId })
            .IsUnique()
            .HasFilter("status IN ('PENDING','READY')")
            .HasDatabaseName("reservations_user_book_active_unique");

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Book)
            .WithMany()
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK opcional: se llena cuando la reserva se concreta en un préstamo
        builder.HasOne(r => r.FulfilledByLoan)
            .WithMany()
            .HasForeignKey(r => r.FulfilledByLoanId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
