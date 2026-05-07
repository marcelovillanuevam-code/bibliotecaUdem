using System.Text.Json;
using System.Text.Json.Serialization;
using Biblioteca.API.Controllers;
using Biblioteca.API.Services;
using Biblioteca.Application.DTOs.Returns;
using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Repositories;
using Biblioteca.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Biblioteca.Tests.Tests;

public sealed class DevolucionesTests
{
    // ---------------------------------------------------------------------------
    // GET /api/devoluciones — capa de repositorio
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ListAsync_sin_devoluciones_devuelve_lista_vacia()
    {
        await using var db = TestDbContextFactory.Create();
        var repo = new ReturnRepository(db);

        var result = await repo.ListAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAsync_devuelve_las_devoluciones_guardadas_ordenadas_por_fecha_desc()
    {
        await using var db = TestDbContextFactory.Create();

        var usuario = TestData.NewUsuario();
        var libro = TestData.NewLibro();
        var copia = new BookCopy
        {
            Id = Guid.NewGuid(),
            BookId = libro.Id,
            Barcode = "BC-TEST-001",
            Status = BookCopyStatus.Available,
            AcquiredAt = FixedDateTimeProvider.Fixed,
            CreatedAt = FixedDateTimeProvider.Fixed,
            UpdatedAt = FixedDateTimeProvider.Fixed
        };
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = usuario.Id,
            BookCopyId = copia.Id,
            LoanedAt = FixedDateTimeProvider.Fixed,
            DueAt = FixedDateTimeProvider.Fixed.AddDays(14),
            Status = LoanStatus.RETURNED,
            IssuedByUserId = usuario.Id
        };

        db.Users.Add(usuario);
        db.Books.Add(libro);
        db.BookCopies.Add(copia);
        db.Loans.Add(loan);

        var returnOld = new Return
        {
            Id = Guid.NewGuid(),
            LoanId = loan.Id,
            ReturnedAt = FixedDateTimeProvider.Fixed.AddDays(-2),
            Condition = ReturnCondition.OK,
            ReceivedByUserId = usuario.Id
        };
        var returnNew = new Return
        {
            Id = Guid.NewGuid(),
            LoanId = loan.Id,
            ReturnedAt = FixedDateTimeProvider.Fixed,
            Condition = ReturnCondition.DAMAGED,
            ReceivedByUserId = usuario.Id
        };

        db.Returns.AddRange(returnOld, returnNew);
        await db.SaveChangesAsync();

        var repo = new ReturnRepository(db);
        var result = await repo.ListAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].ReturnedAt.Should().BeAfter(result[1].ReturnedAt, "deben venir ordenados más reciente primero");
    }

    // ---------------------------------------------------------------------------
    // POST /api/devoluciones — deserialización del enum como string
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData("OK", ReturnCondition.OK)]
    [InlineData("DAMAGED", ReturnCondition.DAMAGED)]
    [InlineData("LOST", ReturnCondition.LOST)]
    public void CreateReturnRequest_deserializa_condition_como_string(string conditionStr, ReturnCondition expected)
    {
        var loanId = Guid.NewGuid();
        var json = $$"""{"loanId":"{{loanId}}","condition":"{{conditionStr}}","inspectionNotes":null}""";

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        opts.Converters.Add(new JsonStringEnumConverter());

        var req = JsonSerializer.Deserialize<CreateReturnRequest>(json, opts);

        req.Should().NotBeNull();
        req!.LoanId.Should().Be(loanId);
        req.Condition.Should().Be(expected);
    }

    [Fact]
    public void CreateReturnRequest_sin_JsonStringEnumConverter_falla_con_string()
    {
        var json = """{"loanId":"00000000-0000-0000-0000-000000000001","condition":"OK"}""";

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var act = () => JsonSerializer.Deserialize<CreateReturnRequest>(json, opts);

        act.Should().Throw<JsonException>("sin el converter, 'OK' no puede deserializarse como int enum");
    }

    // ---------------------------------------------------------------------------
    // POST /api/devoluciones — validación de LoanId vacío en el controlador
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_con_LoanId_vacio_devuelve_400_con_mensaje_descriptivo()
    {
        await using var db = TestDbContextFactory.Create();
        var returnService = new StubReturnService();
        var currentUser = new StubCurrentUserService(Guid.NewGuid());
        var controller = new DevolucionesController(returnService, currentUser);

        var request = new CreateReturnRequest { LoanId = Guid.Empty, Condition = ReturnCondition.OK };
        var result = await controller.CreateAsync(request, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var problem = badRequest.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Detail.Should().Contain("loanId");
    }
}

// ---------------------------------------------------------------------------
// Stubs locales para tests del controlador
// ---------------------------------------------------------------------------

file sealed class StubReturnService : IReturnService
{
    public Task<List<ReturnDto>> ListAsync(CancellationToken ct) =>
        Task.FromResult(new List<ReturnDto>());

    public Task<ReturnDto> CreateAsync(CreateReturnRequest request, Guid receivedBy, CancellationToken ct) =>
        throw new InvalidOperationException("No debería llamarse en este test.");

    public Task<ReturnDto?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult<ReturnDto?>(null);
}

file sealed class StubCurrentUserService : Biblioteca.Application.Interfaces.Common.ICurrentUserService
{
    private readonly Guid _id;
    public StubCurrentUserService(Guid id) => _id = id;
    public Guid? CurrentUserId => _id;
}
