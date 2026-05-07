using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class ContactoUsuarioConfig : IEntityTypeConfiguration<ContactoUsuario>
{
    public void Configure(EntityTypeBuilder<ContactoUsuario> builder)
    {
        builder.ToTable("user_contacts", table =>
        {
            table.HasCheckConstraint("chk_user_contacts_type", "type IN ('email', 'phone')");
        });

        builder.HasKey(contacto => contacto.Id);

        builder.Property(contacto => contacto.Id).HasColumnName("id");
        builder.Property(contacto => contacto.UserId).HasColumnName("user_id");
        builder.Property(contacto => contacto.Type).HasColumnName("type").HasMaxLength(20).IsRequired();
        builder.Property(contacto => contacto.Value).HasColumnName("value").HasMaxLength(255).IsRequired();
        builder.Property(contacto => contacto.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false).IsRequired();
        builder.Property(contacto => contacto.IsVerified).HasColumnName("is_verified").HasDefaultValue(false).IsRequired();
        builder.Property(contacto => contacto.VerifiedAt).HasColumnName("verified_at");
        builder.Property(contacto => contacto.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(contacto => contacto.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasIndex(contacto => new { contacto.Type, contacto.Value }).IsUnique();
        builder.HasIndex(contacto => contacto.UserId).HasDatabaseName("idx_user_contacts_userid");

        builder.HasOne(contacto => contacto.User)
            .WithMany(usuario => usuario.Contacts)
            .HasForeignKey(contacto => contacto.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
