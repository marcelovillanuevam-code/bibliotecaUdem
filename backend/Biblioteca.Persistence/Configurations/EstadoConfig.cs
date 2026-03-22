using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class EstadoConfig : IEntityTypeConfiguration<Estado>
{
    public void Configure(EntityTypeBuilder<Estado> builder)
    {
        builder.ToTable("statuses");

        builder.HasKey(estado => estado.Code);

        builder.Property(estado => estado.Code).HasColumnName("code").HasMaxLength(30);
        builder.Property(estado => estado.Description).HasColumnName("description").HasMaxLength(255);
    }
}
