using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.UserId).HasColumnName("user_id");

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasMaxLength(30)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(n => n.Channel)
            .HasColumnName("channel")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(n => n.Subject)
            .HasColumnName("subject")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.Body)
            .HasColumnName("body")
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(n => n.SentAt).HasColumnName("sent_at");

        builder.Property(n => n.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(1000);

        // jsonb para queries eficientes sobre el payload estructurado
        builder.Property(n => n.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("jsonb")
            .IsRequired();

        // Sin soft-delete: es histórico, nunca se borra

        // Índice para el worker que despacha notificaciones pendientes
        builder.HasIndex(n => new { n.Status, n.CreatedAt })
            .HasDatabaseName("idx_notifications_status_created_at");

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
