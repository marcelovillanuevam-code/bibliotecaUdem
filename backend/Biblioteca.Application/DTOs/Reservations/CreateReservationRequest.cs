using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Reservations;

public sealed class CreateReservationRequest
{
    [Required]
    public Guid BookId { get; init; }
}
