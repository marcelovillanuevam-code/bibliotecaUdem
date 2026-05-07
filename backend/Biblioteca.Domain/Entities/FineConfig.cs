namespace Biblioteca.Domain.Entities;

public sealed class FineConfig
{
    public Guid Id { get; set; }
    public decimal LateRatePerDayMxn { get; set; }
    public decimal DamageFlatMxn { get; set; }
    public decimal LossFlatMxn { get; set; }
    public DateTime EffectiveFrom { get; set; }
}
