using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class PerfilUsuarioConfig : IEntityTypeConfiguration<PerfilUsuario>
{
    public void Configure(EntityTypeBuilder<PerfilUsuario> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(perfil => perfil.UserId);

        builder.Property(perfil => perfil.UserId).HasColumnName("user_id");
        builder.Property(perfil => perfil.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(perfil => perfil.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(perfil => perfil.DisplayName).HasColumnName("display_name").HasMaxLength(200);
        builder.Property(perfil => perfil.DocumentType).HasColumnName("document_type").HasMaxLength(50);
        builder.Property(perfil => perfil.DocumentNumber).HasColumnName("document_number").HasMaxLength(100);
        builder.Property(perfil => perfil.BirthDate).HasColumnName("birth_date");
        builder.Property(perfil => perfil.Gender).HasColumnName("gender").HasMaxLength(30);
        builder.Property(perfil => perfil.AddressJson).HasColumnName("address").HasColumnType("jsonb");
        builder.Property(perfil => perfil.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(perfil => perfil.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasOne(perfil => perfil.User)
            .WithOne(usuario => usuario.Profile)
            .HasForeignKey<PerfilUsuario>(perfil => perfil.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
