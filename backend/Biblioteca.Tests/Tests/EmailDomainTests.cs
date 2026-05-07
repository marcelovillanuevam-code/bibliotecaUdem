using System.ComponentModel.DataAnnotations;
using Biblioteca.Application.DTOs.Auth;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Application.Options;
using Biblioteca.Application.Services.Auth;
using Biblioteca.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Biblioteca.Tests.Tests;

// Email domain: RegisterAsync debe rechazar correos que no sean @udem.edu
public sealed class EmailDomainTests
{
    [Theory]
    [InlineData("student@gmail.com")]
    [InlineData("admin@hotmail.com")]
    [InlineData("fake@udem.edu.evil.com")]
    public async Task RegisterAsync_lanza_ValidationException_para_emails_fuera_del_dominio(string email)
    {
        var authOptions = Options.Create(new AuthOptions { AllowedEmailDomain = "udem.edu" });
        // Las dependencias nulas no se alcanzan: la excepción se lanza antes de cualquier llamada al repo
        var service = new AuthService(
            new ThrowingUserRepository(),
            null!,
            null!,
            null!,
            authOptions,
            null!);

        var request = new RegisterUserRequest
        {
            Username = "testuser",
            Email = email,
            Password = "Password1",
            FirstName = "Test",
            LastName = "User"
        };

        var act = () => service.RegisterAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*udem.edu*",
                "el mensaje debe indicar el dominio permitido");
    }

    [Fact]
    public async Task RegisterAsync_acepta_correo_con_dominio_correcto()
    {
        var authOptions = Options.Create(new AuthOptions { AllowedEmailDomain = "udem.edu" });
        var service = new AuthService(
            new ThrowingUserRepository(),
            null!,
            null!,
            null!,
            authOptions,
            null!);

        var request = new RegisterUserRequest
        {
            Username = "valid",
            Email = "valid@udem.edu",
            Password = "Password1",
            FirstName = "Valid",
            LastName = "User"
        };

        // La validación de dominio pasa; la siguiente llamada (EnsureReferenceDataAsync) lanza NotImplementedException
        // — eso significa que el guard de dominio no rechazó el correo
        var act = () => service.RegisterAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<NotImplementedException>(
            "la validación de dominio pasó y luego se alcanzó el repositorio stub");
    }

    // BUG-09 + Lote 6: el dominio debe aceptarse sin importar la capitalización del input
    [Theory]
    [InlineData("estudiante@UDEM.EDU")]
    [InlineData("admin@Udem.Edu")]
    [InlineData("TEST@udem.edu")]
    public async Task RegisterAsync_acepta_correo_con_dominio_en_cualquier_capitalizacion(string email)
    {
        var authOptions = Options.Create(new AuthOptions { AllowedEmailDomain = "udem.edu" });
        var service = new AuthService(
            new ThrowingUserRepository(),
            null!,
            null!,
            null!,
            authOptions,
            null!);

        var request = new RegisterUserRequest
        {
            Username = "valid",
            Email = email,
            Password = "Password1",
            FirstName = "Valid",
            LastName = "User"
        };

        var act = () => service.RegisterAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<NotImplementedException>(
            "la validación de dominio pasó (case-insensitive) y se alcanzó el repositorio stub");
    }

    // Stub cuyo único propósito es detectar si el repositorio fue alcanzado
    private sealed class ThrowingUserRepository : IUsuarioRepository
    {
        public Task EnsureReferenceDataAsync(CancellationToken ct) => throw new NotImplementedException();

        public Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken ct) => throw new NotImplementedException();
        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
        public Task<Usuario?> GetByIdForUpdateAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
        public Task<Usuario?> GetByUsernameAsync(string username, CancellationToken ct) => throw new NotImplementedException();
        public Task<bool> UsernameExistsAsync(string username, CancellationToken ct, Guid? excludedUserId = null) => throw new NotImplementedException();
        public Task<bool> EmailExistsAsync(string email, CancellationToken ct, Guid? excludedUserId = null) => throw new NotImplementedException();
        public Task<Rol?> GetRoleByCodeAsync(string roleCode, CancellationToken ct) => throw new NotImplementedException();
        public Task<Usuario> AddAsync(Usuario usuario, CancellationToken ct) => throw new NotImplementedException();
        public Task<Usuario> RegisterAsync(Usuario u, AutenticacionUsuario auth, PerfilUsuario profile, ContactoUsuario emailContact, UsuarioRol userRole, CancellationToken ct) => throw new NotImplementedException();
        public Task UpdateAuthAsync(AutenticacionUsuario auth, CancellationToken ct) => throw new NotImplementedException();
        public Task SaveChangesAsync(CancellationToken ct) => throw new NotImplementedException();
        public Task SaveChangesInTransactionAsync(CancellationToken ct) => throw new NotImplementedException();
    }
}
