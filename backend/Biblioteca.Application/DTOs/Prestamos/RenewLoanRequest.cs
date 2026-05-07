using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Prestamos;

public sealed class RenewLoanRequest
{
    [StringLength(500)]
    public string? Notes { get; init; }
}
