using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class AutorConfig : IEntityTypeConfiguration<Autor>
{
    public void Configure(EntityTypeBuilder<Autor> builder)
    {
        builder.ToTable("authors");

        builder.HasKey(autor => autor.Id);

        builder.Property(autor => autor.Id).HasColumnName("id");
        builder.Property(autor => autor.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
        builder.Property(autor => autor.BirthDate).HasColumnName("birth_date");
        builder.Property(autor => autor.BioJson).HasColumnName("bio").HasColumnType("jsonb");
        builder.Property(autor => autor.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(autor => autor.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasIndex(autor => autor.FullName).HasDatabaseName("idx_authors_name");
    }
}
