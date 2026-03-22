namespace Biblioteca.Domain.Entities;

public sealed class RegistroAuditoria
{
    public Guid Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public Guid? RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? ChangedDataJson { get; set; }
    public string? Reason { get; set; }
}
