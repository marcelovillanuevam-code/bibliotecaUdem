using System.ComponentModel.DataAnnotations;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.DTOs.Returns;

public sealed class CreateReturnRequest
{
    [Required]
    public Guid LoanId { get; init; }

    [Required]
    public ReturnCondition Condition { get; init; }

    [StringLength(1000)]
    public string? InspectionNotes { get; init; }
}
