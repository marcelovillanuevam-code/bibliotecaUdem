using Biblioteca.Application.DTOs.Chatbot;
using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Loans;
using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Application.Services.Chatbot;
using Biblioteca.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Biblioteca.Tests.Tests;

// Lote MA-6: verifica que RuleBasedChatbotProvider clasifica intents y devuelve respuestas correctas.
public sealed class RuleBasedChatbotTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    // ── Fakes ────────────────────────────────────────────────────────────────

    private sealed class FakeLibroRepo(params LibroDto[] books) : ILibroRepository
    {
        public Task<IReadOnlyCollection<LibroDto>> GetAllAsync(GetLibrosRequest request, CancellationToken ct)
        {
            var q = books.AsEnumerable();
            if (!string.IsNullOrEmpty(request.Title))
                q = q.Where(b => b.Title.Contains(request.Title, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult<IReadOnlyCollection<LibroDto>>(q.ToList());
        }

        public Task<Libro?> GetByIdAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
        public Task<Libro?> GetByIdForUpdateAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
        public Task<bool> IsbnExistsAsync(string isbn, CancellationToken ct, Guid? excludedBookId = null) => throw new NotImplementedException();
        public Task<Autor?> GetAuthorByFullNameAsync(string fullName, CancellationToken ct) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<Materia>> GetSubjectsByCodesAsync(IReadOnlyCollection<string> subjectCodes, CancellationToken ct) => throw new NotImplementedException();
        public Task<Libro> AddAsync(Libro libro, CancellationToken ct) => throw new NotImplementedException();
        public Task SaveChangesAsync(CancellationToken ct) => throw new NotImplementedException();
        public void Remove(Libro libro) => throw new NotImplementedException();
    }

    private sealed class FakeFineRepo(params Fine[] fines) : IFineRepository
    {
        public Task<IReadOnlyCollection<Fine>> GetByUserAsync(Guid userId, FineStatus? statusFilter, CancellationToken ct)
        {
            var q = fines.Where(f => f.UserId == userId);
            if (statusFilter.HasValue) q = q.Where(f => f.Status == statusFilter.Value);
            return Task.FromResult<IReadOnlyCollection<Fine>>(q.ToList());
        }

        public Task<Fine?> GetByIdAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
        public Task<bool> HasPendingByUserAsync(Guid userId, CancellationToken ct) => throw new NotImplementedException();
        public Task<Fine> AddAsync(Fine fine, CancellationToken ct) => throw new NotImplementedException();
        public Task UpdateAsync(Fine fine, CancellationToken ct) => throw new NotImplementedException();
    }

    private sealed class FakeLoanRepo(params Loan[] loans) : ILoanRepository
    {
        public Task<IReadOnlyCollection<Loan>> GetByUserAsync(Guid userId, LoanStatus? statusFilter, CancellationToken ct)
        {
            var q = loans.Where(l => l.UserId == userId);
            if (statusFilter.HasValue) q = q.Where(l => l.Status == statusFilter.Value);
            return Task.FromResult<IReadOnlyCollection<Loan>>(q.OrderByDescending(l => l.LoanedAt).ToList());
        }

        public Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
        public Task<Loan?> GetByIdForUpdateAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
        public Task<Loan?> GetActiveByBookCopyAsync(Guid bookCopyId, CancellationToken ct) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<Loan>> GetActiveByUserAsync(Guid userId, CancellationToken ct) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<Loan>> GetAllAsync(LoanStatus? statusFilter, CancellationToken ct) => throw new NotImplementedException();
        public Task<Loan> AddAsync(Loan loan, CancellationToken ct) => throw new NotImplementedException();
        public Task UpdateAsync(Loan loan, CancellationToken ct) => throw new NotImplementedException();
        public Task<int> CountActiveByUserAsync(Guid userId, CancellationToken ct) => throw new NotImplementedException();
        public Task<IReadOnlyCollection<Loan>> GetOverdueActiveAsync(CancellationToken ct) => throw new NotImplementedException();
    }

    private static RuleBasedChatbotProvider Build(
        ILibroRepository? libros = null,
        IFineRepository? multas = null,
        ILoanRepository? loans = null) =>
        new(libros ?? new FakeLibroRepo(), multas ?? new FakeFineRepo(), loans ?? new FakeLoanRepo());

    // ── Helper data ───────────────────────────────────────────────────────────

    private static LibroDto NewLibroDto(string title, string author = "Autor") =>
        new(Guid.NewGuid(), title, null, null, null, null, null, "es",
            [new LibroAutorDto(Guid.NewGuid(), author, null)],
            [], DateTime.UtcNow, DateTime.UtcNow, 3, 2);

    private static Fine NewPendingFine(decimal amount = 150m) => new()
    {
        Id = Guid.NewGuid(),
        UserId = UserId,
        ReturnId = Guid.NewGuid(),
        Amount = amount,
        Reason = FineReason.LATE,
        Status = FineStatus.PENDING,
        CreatedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
        DaysLate = 5
    };

    private static Loan NewActiveLoan(string bookTitle, DateTime dueAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = UserId,
        BookCopyId = Guid.NewGuid(),
        LoanedAt = dueAt.AddDays(-7),
        DueAt = dueAt,
        Status = LoanStatus.ACTIVE,
        IssuedByUserId = Guid.NewGuid(),
        BookCopy = new BookCopy
        {
            Id = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            Barcode = "BC-TEST",
            AcquiredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Book = new Libro { Id = Guid.NewGuid(), Title = bookTitle, Language = "es",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        }
    };

    // ── Tests: búsqueda ───────────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_libro_existente_devuelve_titulo_y_acciones()
    {
        var provider = Build(libros: new FakeLibroRepo(NewLibroDto("Harry Potter", "J.K. Rowling")));

        var res = await provider.AskAsync("buscar harry potter", UserId, default);

        res.Reply.Should().Contain("Harry Potter");
        res.Reply.Should().Contain("J.K. Rowling");
        res.Actions.Should().HaveCount(1);
        res.Actions![0].Label.Should().Contain("Harry Potter");
        res.Actions[0].Url.Should().Be("/dashboard/catalogo");
    }

    [Fact]
    public async Task Buscar_devuelve_maximo_3_resultados()
    {
        var libros = Enumerable.Range(1, 5).Select(i => NewLibroDto($"Libro {i}")).ToArray();
        var provider = Build(libros: new FakeLibroRepo(libros));

        var res = await provider.AskAsync("buscar libro", UserId, default);

        res.Actions.Should().HaveCount(3);
    }

    [Fact]
    public async Task Buscar_sin_resultados_devuelve_mensaje_de_no_encontrado()
    {
        var provider = Build(libros: new FakeLibroRepo());

        var res = await provider.AskAsync("buscar cien años de soledad", UserId, default);

        res.Reply.Should().Contain("No encontré");
        res.Actions.Should().BeNull();
    }

    // ── Tests: multas ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Mis_multas_sin_multas_pendientes_devuelve_mensaje_limpio()
    {
        var provider = Build(multas: new FakeFineRepo());

        var res = await provider.AskAsync("mis multas", UserId, default);

        res.Reply.Should().Be("Sin multas pendientes.");
        res.Actions.Should().BeNull();
    }

    [Fact]
    public async Task Mis_multas_con_multa_pendiente_lista_monto()
    {
        var provider = Build(multas: new FakeFineRepo(NewPendingFine(150m)));

        var res = await provider.AskAsync("mis multas", UserId, default);

        res.Reply.Should().Contain("150");
        res.Reply.Should().Contain("1 multa");
    }

    [Fact]
    public async Task Tengo_multas_es_alias_de_mis_multas()
    {
        var provider = Build(multas: new FakeFineRepo(NewPendingFine()));

        var res = await provider.AskAsync("tengo multas", UserId, default);

        res.Reply.Should().Contain("multa");
    }

    // ── Tests: préstamos ──────────────────────────────────────────────────────

    [Fact]
    public async Task Mis_prestamos_sin_activos_devuelve_mensaje_vacio()
    {
        var provider = Build(loans: new FakeLoanRepo());

        var res = await provider.AskAsync("mis préstamos", UserId, default);

        res.Reply.Should().Be("No tenés préstamos activos.");
        res.Actions.Should().BeNull();
    }

    [Fact]
    public async Task Mis_prestamos_activos_lista_titulo_y_fecha_de_vencimiento()
    {
        var due = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var provider = Build(loans: new FakeLoanRepo(NewActiveLoan("El Aleph", due)));

        var res = await provider.AskAsync("mis préstamos", UserId, default);

        res.Reply.Should().Contain("El Aleph");
        res.Reply.Should().Contain("15/01/2026");
    }

    [Fact]
    public async Task Prestamo_vencido_muestra_alerta_overdue()
    {
        var due = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var loan = NewActiveLoan("Don Quijote", due);
        loan.Status = LoanStatus.OVERDUE;
        var provider = Build(loans: new FakeLoanRepo(loan));

        var res = await provider.AskAsync("mis préstamos", UserId, default);

        res.Reply.Should().Contain("VENCIDO");
    }

    // ── Tests: FAQ ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Horario_devuelve_horarios_de_atencion()
    {
        var res = await Build().AskAsync("horario", UserId, default);

        res.Reply.Should().Contain("7:00");
        res.Reply.Should().Contain("22:00");
        res.Actions.Should().BeNull();
    }

    [Fact]
    public async Task Ubicacion_devuelve_direccion()
    {
        var res = await Build().AskAsync("ubicación", UserId, default);

        res.Reply.Should().Contain("UDEM");
        res.Actions.Should().BeNull();
    }

    [Fact]
    public async Task Contacto_devuelve_correo_y_telefono()
    {
        var res = await Build().AskAsync("contacto", UserId, default);

        res.Reply.Should().Contain("biblioteca@udem.edu");
    }

    // ── Tests: default ────────────────────────────────────────────────────────

    [Fact]
    public async Task Mensaje_desconocido_devuelve_lista_de_comandos()
    {
        var res = await Build().AskAsync("hola que tal", UserId, default);

        res.Reply.Should().Contain("No entendí");
        res.Reply.Should().Contain("buscar");
    }
}
