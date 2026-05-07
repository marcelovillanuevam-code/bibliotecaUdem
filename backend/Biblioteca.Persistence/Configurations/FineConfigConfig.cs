using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Alias para evitar colisión entre la entidad FineConfig y esta clase de configuración EF
using FineConfigEntity = Biblioteca.Domain.Entities.FineConfig;

namespace Biblioteca.Persistence.Configurations;

public sealed class FineConfigConfig : IEntityTypeConfiguration<FineConfigEntity>
{
    public void Configure(EntityTypeBuilder<FineConfigEntity> builder)
    {
        builder.ToTable("fine_configs");

        builder.HasKey(fc => fc.Id);

        builder.Property(fc => fc.Id).HasColumnName("id");

        builder.Property(fc => fc.LateRatePerDayMxn)
            .HasColumnName("late_rate_per_day_mxn")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(fc => fc.DamageFlatMxn)
            .HasColumnName("damage_flat_mxn")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(fc => fc.LossFlatMxn)
            .HasColumnName("loss_flat_mxn")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(fc => fc.EffectiveFrom)
            .HasColumnName("effective_from")
            .IsRequired();

        // Sin soft-delete: es config histórica, los registros se acumulan
        builder.HasIndex(fc => fc.EffectiveFrom)
            .HasDatabaseName("idx_fine_configs_effective_from");
    }
}
