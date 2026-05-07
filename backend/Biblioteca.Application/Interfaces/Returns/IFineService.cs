using Biblioteca.Application.DTOs.Returns;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Returns;

public interface IFineService
{
    Task<FineDto?> GetByIdAsync(Guid id, Guid requesterUserId, bool isAdminOrLibrarian, CancellationToken ct);
    Task<IReadOnlyCollection<FineDto>> GetByUserAsync(Guid userId, FineStatus? statusFilter, CancellationToken ct);
    Task<FineDto> ConfirmPaymentAsync(Guid fineId, ConfirmPaymentRequest request, Guid paidBy, CancellationToken ct);
    Task<FineDto> WaiveAsync(Guid fineId, WaiveFineRequest request, Guid waivedBy, CancellationToken ct);
}
