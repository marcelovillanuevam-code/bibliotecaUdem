using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Persistence.Configurations;

public sealed class RegistroAuditoriaConfig : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("audit_logs", table =>
        {
            table.HasCheckConstraint("chk_audit_logs_action", "action IN ('INSERT', 'UPDATE', 'DELETE')");
        });

        builder.HasKey(registro => registro.Id);

        builder.Property(registro => registro.Id).HasColumnName("id");
        builder.Property(registro => registro.TableName).HasColumnName("table_name").HasMaxLength(128).IsRequired();
        builder.Property(registro => registro.RecordId).HasColumnName("record_id");
        builder.Property(registro => registro.Action).HasColumnName("action").HasMaxLength(10).IsRequired();
        builder.Property(registro => registro.PerformedBy).HasColumnName("performed_by");
        builder.Property(registro => registro.PerformedAt).HasColumnName("performed_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(registro => registro.ChangedDataJson).HasColumnName("changed_data").HasColumnType("jsonb");
        builder.Property(registro => registro.Reason).HasColumnName("reason");

        builder.HasIndex(registro => registro.PerformedAt).HasDatabaseName("idx_audit_performed_at");
    }
}
