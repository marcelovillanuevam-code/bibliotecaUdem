using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class MateriaConfig : IEntityTypeConfiguration<Materia>
{
    public void Configure(EntityTypeBuilder<Materia> builder)
    {
        builder.ToTable("subjects");

        builder.HasKey(materia => materia.Id);

        builder.Property(materia => materia.Id).HasColumnName("id");
        builder.Property(materia => materia.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
        builder.Property(materia => materia.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        builder.Property(materia => materia.Description).HasColumnName("description");
        builder.Property(materia => materia.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasIndex(materia => materia.Code).IsUnique();
        builder.HasIndex(materia => materia.Name).HasDatabaseName("idx_subjects_name");
    }
}
