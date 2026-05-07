using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class RolConfig : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(rol => rol.Id);

        builder.Property(rol => rol.Id).HasColumnName("id");
        builder.Property(rol => rol.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(rol => rol.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
        builder.Property(rol => rol.Description).HasColumnName("description");
        builder.Property(rol => rol.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasIndex(rol => rol.Code).IsUnique();
    }
}
