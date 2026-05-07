using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Returns;

public sealed class UpdateFineConfigRequest
{
    [Required]
    [Range(0.01, 9999.99)]
    public decimal LateRatePerDayMxn { get; init; }

    [Required]
    [Range(0.01, 99999.99)]
    public decimal DamageFlatMxn { get; init; }

    [Required]
    [Range(0.01, 99999.99)]
    public decimal LossFlatMxn { get; init; }

    [Required]
    public DateTime EffectiveFrom { get; init; }
}
