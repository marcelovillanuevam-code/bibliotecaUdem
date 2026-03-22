using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint("chk_username_format", "username ~ '^[A-Za-z0-9._@-]{3,100}$'");
        });

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Id)
            .HasColumnName("id");

        builder.Property(usuario => usuario.Username)
            .HasColumnName("username")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(usuario => usuario.UsernameLower)
            .HasColumnName("username_lower")
            .HasMaxLength(100)
            .HasComputedColumnSql("lower(username)", stored: true);

        builder.Property(usuario => usuario.StatusCode)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasDefaultValue("pending_verification")
            .IsRequired();

        builder.Property(usuario => usuario.PreferredLocale)
            .HasColumnName("preferred_locale")
            .HasMaxLength(10)
            .HasDefaultValue("es_MX");

        builder.Property(usuario => usuario.MetadataJson)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(usuario => usuario.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(usuario => usuario.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(usuario => usuario.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasIndex(usuario => usuario.Username).IsUnique();
        builder.HasIndex(usuario => usuario.StatusCode).HasDatabaseName("idx_users_status");
        builder.HasIndex(usuario => usuario.UsernameLower).HasDatabaseName("idx_users_username_lower");

        builder.HasOne(usuario => usuario.Status)
            .WithMany(status => status.Users)
            .HasForeignKey(usuario => usuario.StatusCode)
            .HasPrincipalKey(status => status.Code);
    }
}
