using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Repositories;
using Biblioteca.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Biblioteca.Tests.Tests;

// Lote MA-1: verifica que LibroRepository proyecta AvailableCopies correctamente
// y que cambiar el status de una copia actualiza el conteo. Fallaría si se revierte MA-4.
public sealed class BookAvailabilityTests
{
    private static BookCopy NewCopy(Guid bookId, string status, string barcode) => new()
    {
        Id = Guid.NewGuid(),
        BookId = bookId,
        Barcode = barcode,
        Status = status,
        AcquiredAt = FixedDateTimeProvider.Fixed,
        CreatedAt = FixedDateTimeProvider.Fixed,
        UpdatedAt = FixedDateTimeProvider.Fixed
    };

    [Fact]
    public async Task GetAllAsync_devuelve_AvailableCopies_correcto()
    {
        await using var db = TestDbContextFactory.Create();
        var libro = TestData.NewLibro("Fundación");
        db.Books.Add(libro);

        db.BookCopies.Add(NewCopy(libro.Id, BookCopyStatus.Available, "BC-001"));
        db.BookCopies.Add(NewCopy(libro.Id, BookCopyStatus.Available, "BC-002"));
        db.BookCopies.Add(NewCopy(libro.Id, BookCopyStatus.Maintenance, "BC-003"));
        await db.SaveChangesAsync();

        var repo = new LibroRepository(db, new FixedDateTimeProvider());
        var result = await repo.GetAllAsync(new GetLibrosRequest(), CancellationToken.None);

        var dto = result.Should().ContainSingle().Subject;
        dto.TotalCopies.Should().Be(3);
        dto.AvailableCopies.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_AvailableCopies_disminuye_al_cambiar_status_de_copia()
    {
        await using var db = TestDbContextFactory.Create();
        var libro = TestData.NewLibro("El Aleph");
        db.Books.Add(libro);

        var copy1 = NewCopy(libro.Id, BookCopyStatus.Available, "BC-A01");
        var copy2 = NewCopy(libro.Id, BookCopyStatus.Available, "BC-A02");
        db.BookCopies.AddRange(copy1, copy2);
        await db.SaveChangesAsync();

        var repo = new LibroRepository(db, new FixedDateTimeProvider());

        var antes = await repo.GetAllAsync(new GetLibrosRequest(), CancellationToken.None);
        antes.Should().ContainSingle().Which.AvailableCopies.Should().Be(2);

        // Cambiar una copia a LOST
        copy1.Status = BookCopyStatus.Lost;
        await db.SaveChangesAsync();

        var despues = await repo.GetAllAsync(new GetLibrosRequest(), CancellationToken.None);
        despues.Should().ContainSingle().Which.AvailableCopies.Should().Be(1);
    }
}
