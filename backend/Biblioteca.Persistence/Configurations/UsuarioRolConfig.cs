using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class UsuarioRolConfig : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(usuarioRol => new { usuarioRol.UserId, usuarioRol.RoleId });

        builder.Property(usuarioRol => usuarioRol.UserId).HasColumnName("user_id");
        builder.Property(usuarioRol => usuarioRol.RoleId).HasColumnName("role_id");
        builder.Property(usuarioRol => usuarioRol.AssignedBy).HasColumnName("assigned_by");
        builder.Property(usuarioRol => usuarioRol.AssignedAt).HasColumnName("assigned_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasIndex(usuarioRol => usuarioRol.RoleId).HasDatabaseName("idx_user_roles_roleid");

        builder.HasOne(usuarioRol => usuarioRol.User)
            .WithMany(usuario => usuario.Roles)
            .HasForeignKey(usuarioRol => usuarioRol.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(usuarioRol => usuarioRol.Role)
            .WithMany(rol => rol.Users)
            .HasForeignKey(usuarioRol => usuarioRol.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
