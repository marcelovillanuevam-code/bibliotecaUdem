using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class TokenRestablecimientoContrasenaConfig : IEntityTypeConfiguration<TokenRestablecimientoContrasena>
{
    public void Configure(EntityTypeBuilder<TokenRestablecimientoContrasena> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.Id).HasColumnName("id");
        builder.Property(token => token.UserId).HasColumnName("user_id");
        builder.Property(token => token.Token).HasColumnName("token").HasMaxLength(255).IsRequired();
        builder.Property(token => token.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(token => token.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(token => token.UsedAt).HasColumnName("used_at");

        builder.HasIndex(token => token.Token).IsUnique();
        builder.HasIndex(token => token.UserId).HasDatabaseName("fk_prt_user");

        builder.HasOne(token => token.User)
            .WithMany(usuario => usuario.PasswordResetTokens)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
