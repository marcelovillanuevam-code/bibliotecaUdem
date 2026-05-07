using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Tests.Helpers;

// Stub de ICurrentUserService para tests (ningún usuario autenticado → no genera audit entries)
internal sealed class NullCurrentUserService : ICurrentUserService
{
    public Guid? CurrentUserId => null;
}

// IDateTimeProvider con tiempo fijo para assertions deterministas
internal sealed class FixedDateTimeProvider : IDateTimeProvider
{
    public static readonly DateTime Fixed = new(2026, 5, 7, 12, 0, 0, DateTimeKind.Utc);
    public DateTime UtcNow => Fixed;
}

// Crea un BibliotecaDbContext InMemory con un nombre de DB único por test
internal static class TestDbContextFactory
{
    public static BibliotecaDbContext Create(string? dbName = null)
    {
        var name = dbName ?? Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<BibliotecaDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new BibliotecaDbContext(options, new NullCurrentUserService());
    }
}

// Libro mínimo válido para tests
internal static class TestData
{
    public static Libro NewLibro(string title = "Test Book") => new()
    {
        Id = Guid.NewGuid(),
        Title = title,
        Language = "es",
        CreatedAt = FixedDateTimeProvider.Fixed,
        UpdatedAt = FixedDateTimeProvider.Fixed
    };

    public static Usuario NewUsuario(string username = "testuser") => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        StatusCode = "active",
        PreferredLocale = "es_MX",
        CreatedAt = FixedDateTimeProvider.Fixed,
        UpdatedAt = FixedDateTimeProvider.Fixed
    };

    public static ContactoUsuario NewEmailContact(Guid userId, string email = "test@udem.edu") => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Type = "email",
        Value = email,
        IsPrimary = true,
        IsVerified = false,
        CreatedAt = FixedDateTimeProvider.Fixed,
        UpdatedAt = FixedDateTimeProvider.Fixed
    };
}
