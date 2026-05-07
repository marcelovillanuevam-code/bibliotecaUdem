using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Prestamos;

public sealed class CreateLoanRequest
{
    [Required]
    public Guid UserId { get; init; }

    [Required]
    public Guid BookCopyId { get; init; }
}
