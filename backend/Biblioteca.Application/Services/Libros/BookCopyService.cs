using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Exceptions;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Services.Libros;

public sealed class BookCopyService(
    IBookCopyRepository bookCopyRepository,
    ILibroRepository libroRepository,
    IDateTimeProvider dateTimeProvider,
    IMapper mapper) : IBookCopyService
{
    public async Task<IReadOnlyCollection<BookCopyDto>> GetByBookIdAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var copies = await bookCopyRepository.GetByBookIdAsync(bookId, cancellationToken);
        return mapper.Map<IReadOnlyCollection<BookCopyDto>>(copies);
    }

    public async Task<BookCopyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var copy = await bookCopyRepository.GetByIdAsync(id, cancellationToken);
        return copy is null ? null : mapper.Map<BookCopyDto>(copy);
    }

    public async Task<BookCopyDto> CreateAsync(Guid bookId, CreateBookCopyRequest request, CancellationToken cancellationToken)
    {
        _ = await libroRepository.GetByIdAsync(bookId, cancellationToken)
            ?? throw new NotFoundException("No se encontró el libro solicitado.");

        var normalizedBarcode = request.Barcode.Trim().ToUpperInvariant();

        if (await bookCopyRepository.BarcodeExistsAsync(normalizedBarcode, cancellationToken))
            throw new ConflictException("El código de barras ya está registrado.");

        var now = dateTimeProvider.UtcNow;

        var copy = new BookCopy
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            Barcode = normalizedBarcode,
            LocationId = request.LocationId,
            Status = NormalizeStatus(request.Status),
            Condition = NormalizeCondition(request.Condition),
            AcquiredAt = request.AcquiredAt == default ? now : request.AcquiredAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await bookCopyRepository.AddAsync(copy, cancellationToken);
        return mapper.Map<BookCopyDto>(created);
    }

    public async Task<BookCopyDto> UpdateAsync(Guid id, UpdateBookCopyRequest request, CancellationToken cancellationToken)
    {
        var copy = await bookCopyRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("No se encontró el ejemplar solicitado.");

        copy.Status = NormalizeStatus(request.Status);
        copy.LocationId = request.LocationId;
        copy.Condition = NormalizeCondition(request.Condition);
        copy.UpdatedAt = dateTimeProvider.UtcNow;

        await bookCopyRepository.SaveChangesAsync(cancellationToken);

        var updated = await bookCopyRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("No se pudo recuperar el ejemplar actualizado.");

        return mapper.Map<BookCopyDto>(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var copy = await bookCopyRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("No se encontró el ejemplar solicitado.");

        bookCopyRepository.SoftDelete(copy);
        await bookCopyRepository.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeStatus(string? status) =>
        string.IsNullOrWhiteSpace(status)
            ? BookCopyStatus.Available
            : BookCopyStatus.IsValid(status.Trim())
                ? status.Trim().ToUpperInvariant()
                : throw new ValidationException("Estado no válido. Valores permitidos: AVAILABLE, MAINTENANCE, LOST, RETIRED.");

    private static string? NormalizeCondition(string? condition) =>
        string.IsNullOrWhiteSpace(condition)
            ? null
            : BookCopyCondition.IsValid(condition.Trim())
                ? condition.Trim().ToUpperInvariant()
                : throw new ValidationException("Condición no válida. Valores permitidos: NEW, GOOD, WORN, DAMAGED.");
}
