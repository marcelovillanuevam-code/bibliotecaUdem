using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class SesionConfig : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> builder)
    {
        builder.ToTable("sessions");

        builder.HasKey(sesion => sesion.Id);

        builder.Property(sesion => sesion.Id).HasColumnName("id");
        builder.Property(sesion => sesion.UserId).HasColumnName("user_id");
        builder.Property(sesion => sesion.SessionToken).HasColumnName("session_token").HasMaxLength(255).IsRequired();
        builder.Property(sesion => sesion.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(sesion => sesion.UserAgent).HasColumnName("user_agent").HasMaxLength(512);
        builder.Property(sesion => sesion.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(sesion => sesion.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(sesion => sesion.RevokedAt).HasColumnName("revoked_at");

        builder.HasIndex(sesion => sesion.SessionToken).IsUnique();
        builder.HasIndex(sesion => sesion.UserId).HasDatabaseName("idx_sessions_userid");

        builder.HasOne(sesion => sesion.User)
            .WithMany(usuario => usuario.Sessions)
            .HasForeignKey(sesion => sesion.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
