using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Exceptions;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Services.Libros;
using Biblioteca.Domain.Entities;
using Biblioteca.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Biblioteca.Tests.Tests;

// BookCopy 409: Eliminar un libro con ejemplares activos debe lanzar ConflictException
public sealed class BookCopy409Tests
{
    [Fact]
    public async Task DeleteAsync_lanza_ConflictException_cuando_existen_ejemplares_activos()
    {
        var libro = TestData.NewLibro("Fundación");
        var libroRepo = new StubLibroRepo(libro);
        var copyRepo = new StubBookCopyRepoWithActiveCopies();
        var service = new LibroService(libroRepo, copyRepo, new FixedDateTimeProvider(), null!);

        var act = () => service.DeleteAsync(libro.Id, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*ejemplares activos*");
    }

    // Stub: siempre devuelve el libro indicado en las consultas de escritura
    private sealed class StubLibroRepo(Libro libro) : ILibroRepository
    {
        public Task<Libro?> GetByIdForUpdateAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<Libro?>(libro);

        public Task<IReadOnlyCollection<LibroDto>> GetAllAsync(GetLibrosRequest request, CancellationToken ct) =>
            throw new NotImplementedException();
        public Task<Libro?> GetByIdAsync(Guid id, CancellationToken ct) =>
            throw new NotImplementedException();
        public Task<bool> IsbnExistsAsync(string isbn, CancellationToken ct, Guid? excludedBookId = null) =>
            throw new NotImplementedException();
        public Task<Autor?> GetAuthorByFullNameAsync(string fullName, CancellationToken ct) =>
            throw new NotImplementedException();
        public Task<IReadOnlyCollection<Materia>> GetSubjectsByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken ct) =>
            throw new NotImplementedException();
        public Task<Libro> AddAsync(Libro l, CancellationToken ct) =>
            throw new NotImplementedException();
        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
        public void Remove(Libro l) { }
    }

    // Stub: siempre reporta que hay copias activas
    private sealed class StubBookCopyRepoWithActiveCopies : IBookCopyRepository
    {
        public Task<bool> HasActiveCopiesAsync(Guid bookId, CancellationToken ct) =>
            Task.FromResult(true);

        public Task<BookCopy?> GetByIdAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
        public Task<BookCopy?> GetByIdForUpdateAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
        public Task<BookCopy?> GetByBarcodeAsync(string barcode, CancellationToken ct) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<BookCopy>> GetByBookIdAsync(Guid bookId, CancellationToken ct) => throw new NotImplementedException();
        public Task<bool> BarcodeExistsAsync(string barcode, CancellationToken ct, Guid? excludedCopyId = null) => throw new NotImplementedException();
        public Task<BookCopy> AddAsync(BookCopy copy, CancellationToken ct) => throw new NotImplementedException();
        public Task SaveChangesAsync(CancellationToken ct) => throw new NotImplementedException();
        public void SoftDelete(BookCopy copy) => throw new NotImplementedException();
        public Task<int> CountAvailableByBookIdAsync(Guid bookId, CancellationToken ct) => Task.FromResult(0);
    }
}
