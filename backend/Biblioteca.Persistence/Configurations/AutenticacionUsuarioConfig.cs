using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class AutenticacionUsuarioConfig : IEntityTypeConfiguration<AutenticacionUsuario>
{
    public void Configure(EntityTypeBuilder<AutenticacionUsuario> builder)
    {
        builder.ToTable("user_auth");

        builder.HasKey(auth => auth.UserId);

        builder.Property(auth => auth.UserId).HasColumnName("user_id");
        builder.Property(auth => auth.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        builder.Property(auth => auth.PasswordChangedAt).HasColumnName("password_changed_at");
        builder.Property(auth => auth.MfaEnabled).HasColumnName("mfa_enabled").HasDefaultValue(false).IsRequired();
        builder.Property(auth => auth.MfaSecret).HasColumnName("mfa_secret");
        builder.Property(auth => auth.FailedLoginCount).HasColumnName("failed_login_count").HasDefaultValue(0).IsRequired();
        builder.Property(auth => auth.LockedUntil).HasColumnName("locked_until");
        builder.Property(auth => auth.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(auth => auth.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasOne(auth => auth.User)
            .WithOne(usuario => usuario.Auth)
            .HasForeignKey<AutenticacionUsuario>(auth => auth.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
