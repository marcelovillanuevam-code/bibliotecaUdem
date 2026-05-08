using System.Text.Json;
using Biblioteca.Application.DTOs.Reservations;
using Biblioteca.Application.Exceptions;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Biblioteca.Application.Services.Reservations;

public sealed class ReservationService(
    IReservationRepository reservationRepository,
    INotificationRepository notificationRepository,
    IBookCopyRepository bookCopyRepository,
    IUserEligibilityService eligibilityService,
    IDateTimeProvider clock,
    ILogger<ReservationService> logger) : IReservationService
{
    public async Task<ReservationDto> CreateAsync(Guid userId, CreateReservationRequest request, CancellationToken ct)
    {
        var eligibility = await eligibilityService.CheckAsync(userId, ct);
        if (!eligibility.IsEligible)
            throw new ConflictException(eligibility.Reason ?? "Usuario no elegible para reservar.");

        var availableCopies = await bookCopyRepository.CountAvailableByBookIdAsync(request.BookId, ct);
        if (availableCopies > 0)
            throw new ConflictException("Hay copias disponibles, registrá un préstamo en su lugar.");

        var existing = await reservationRepository.GetActiveByUserAndBookAsync(userId, request.BookId, ct);
        if (existing is not null)
            throw new ConflictException("Ya tenés una reserva activa para este título.");

        var maxPosition = await reservationRepository.GetMaxQueuePositionAsync(request.BookId, ct);
        var now = clock.UtcNow;

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BookId = request.BookId,
            QueuePosition = maxPosition + 1,
            Status = ReservationStatus.PENDING,
            CreatedAt = now
        };

        var saved = await reservationRepository.AddAsync(reservation, ct);

        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.RESERVATION_READY,
                Channel = NotificationChannel.IN_APP,
                Subject = "Reserva confirmada",
                Body = $"Tu reserva fue registrada en posición {reservation.QueuePosition}.",
                Status = NotificationStatus.PENDING,
                CreatedAt = now,
                PayloadJson = JsonSerializer.Serialize(new { reservationId = reservation.Id, bookId = request.BookId })
            };
            await notificationRepository.AddAsync(notification, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear notificación para reserva {ReservationId}.", reservation.Id);
        }

        return MapToDto(saved);
    }

    public async Task CancelAsync(Guid reservationId, Guid requestedBy, bool isAdmin, CancellationToken ct)
    {
        var reservation = await reservationRepository.GetByIdForUpdateAsync(reservationId, ct)
            ?? throw new NotFoundException("Reserva no encontrada.");

        if (!isAdmin && reservation.UserId != requestedBy)
            throw new UnauthorizedException("No tiene permisos para cancelar esta reserva.");

        if (reservation.Status != ReservationStatus.PENDING && reservation.Status != ReservationStatus.READY)
            throw new ConflictException("Solo se pueden cancelar reservas en estado PENDING o READY.");

        var cancelledPosition = reservation.QueuePosition;
        var bookId = reservation.BookId;

        reservation.Status = ReservationStatus.CANCELLED;
        await reservationRepository.UpdateAsync(reservation, ct);

        var pending = await reservationRepository.GetByBookAsync(bookId, ReservationStatus.PENDING, ct);
        foreach (var r in pending.Where(r => r.QueuePosition > cancelledPosition))
        {
            r.QueuePosition--;
            await reservationRepository.UpdateAsync(r, ct);
        }
    }

    public async Task<ReservationDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var r = await reservationRepository.GetByIdAsync(id, ct);
        return r is null ? null : MapToDto(r);
    }

    public async Task<IReadOnlyCollection<ReservationDto>> GetByUserAsync(Guid userId, CancellationToken ct)
    {
        var reservations = await reservationRepository.GetByUserAsync(userId, null, ct);
        return reservations.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyCollection<ReservationDto>> GetQueueAsync(Guid bookId, CancellationToken ct)
    {
        var reservations = await reservationRepository.GetByBookAsync(bookId, null, ct);
        return reservations
            .Where(r => r.Status == ReservationStatus.PENDING || r.Status == ReservationStatus.READY)
            .Select(MapToDto).ToList();
    }

    private static ReservationDto MapToDto(Reservation r) => new(
        r.Id,
        r.UserId,
        UserFullName: r.User?.Profile is { } profile
            ? string.IsNullOrWhiteSpace(profile.DisplayName)
                ? $"{profile.FirstName} {profile.LastName}".Trim()
                : profile.DisplayName
            : "N/A",
        r.BookId,
        BookTitle: r.Book?.Title ?? "N/A",
        r.QueuePosition,
        Status: r.Status.ToString(),
        r.CreatedAt,
        r.ReadyAt,
        r.ExpiresAt,
        r.FulfilledAt,
        r.FulfilledByLoanId);
}
