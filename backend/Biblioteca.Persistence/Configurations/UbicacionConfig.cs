using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class UbicacionConfig : IEntityTypeConfiguration<Ubicacion>
{
    public void Configure(EntityTypeBuilder<Ubicacion> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(ubicacion => ubicacion.Id);

        builder.Property(ubicacion => ubicacion.Id).HasColumnName("id");
        builder.Property(ubicacion => ubicacion.LibraryName).HasColumnName("library_name").HasMaxLength(200).IsRequired();
        builder.Property(ubicacion => ubicacion.Section).HasColumnName("section").HasMaxLength(100);
        builder.Property(ubicacion => ubicacion.Shelf).HasColumnName("shelf").HasMaxLength(100);
        builder.Property(ubicacion => ubicacion.Notes).HasColumnName("notes").HasMaxLength(255);
        builder.Property(ubicacion => ubicacion.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    }
}
