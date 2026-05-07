using Biblioteca.Application.Common.Events;
using Biblioteca.Application.DTOs.Returns;
using Biblioteca.Application.Exceptions;
using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Services.Returns;

public sealed class FineService(
    IFineRepository fineRepository,
    IDomainEventDispatcher dispatcher) : IFineService
{
    public async Task<FineDto?> GetByIdAsync(Guid id, Guid requesterUserId, bool isAdminOrLibrarian, CancellationToken ct)
    {
        var fine = await fineRepository.GetByIdAsync(id, ct);
        if (fine is null) return null;
        if (!isAdminOrLibrarian && fine.UserId != requesterUserId)
            throw new UnauthorizedException("No tiene permisos para ver esta multa.");
        return MapToDto(fine);
    }

    public async Task<IReadOnlyCollection<FineDto>> GetByUserAsync(Guid userId, FineStatus? statusFilter, CancellationToken ct)
    {
        var fines = await fineRepository.GetByUserAsync(userId, statusFilter, ct);
        return fines.Select(MapToDto).ToList();
    }

    public async Task<FineDto> ConfirmPaymentAsync(Guid fineId, ConfirmPaymentRequest request, Guid paidBy, CancellationToken ct)
    {
        var fine = await fineRepository.GetByIdAsync(fineId, ct)
            ?? throw new NotFoundException("Multa no encontrada.");

        if (fine.Status != FineStatus.PENDING)
            throw new ConflictException("Solo se pueden pagar multas en estado PENDING.");

        fine.Status = FineStatus.PAID;
        fine.PaidAt = DateTime.UtcNow;
        fine.PaidByUserId = paidBy;

        await fineRepository.UpdateAsync(fine, ct);
        await dispatcher.DispatchAsync(new FinePaid(fine.Id, fine.UserId, paidBy, fine.Amount), ct);

        return MapToDto(fine);
    }

    public async Task<FineDto> WaiveAsync(Guid fineId, WaiveFineRequest request, Guid waivedBy, CancellationToken ct)
    {
        var fine = await fineRepository.GetByIdAsync(fineId, ct)
            ?? throw new NotFoundException("Multa no encontrada.");

        if (fine.Status != FineStatus.PENDING)
            throw new ConflictException("Solo se pueden condonar multas en estado PENDING.");

        fine.Status = FineStatus.WAIVED;
        fine.PaidAt = DateTime.UtcNow;
        fine.PaidByUserId = waivedBy;
        fine.WaivedReason = request.Reason;

        await fineRepository.UpdateAsync(fine, ct);

        return MapToDto(fine);
    }

    private static FineDto MapToDto(Fine f) =>
        new(f.Id, f.ReturnId, f.UserId, f.Reason.ToString(), f.Amount, f.DaysLate, f.Status.ToString(), f.CreatedAt, f.PaidAt, f.PaidByUserId);
}
