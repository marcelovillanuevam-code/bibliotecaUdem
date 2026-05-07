using Biblioteca.Application.DTOs.Returns;

namespace Biblioteca.Application.Interfaces.Returns;

public interface IReturnService
{
    Task<ReturnDto> CreateAsync(CreateReturnRequest request, Guid receivedBy, CancellationToken ct);
    Task<ReturnDto?> GetByIdAsync(Guid id, CancellationToken ct);
}
