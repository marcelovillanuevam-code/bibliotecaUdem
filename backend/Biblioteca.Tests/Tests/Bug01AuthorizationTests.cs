using System.Reflection;
using Biblioteca.API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Biblioteca.Tests.Tests;

// BUG-01: Verifica que los endpoints tienen la política de autorización correcta
public sealed class Bug01AuthorizationTests
{
    // Constantes copiadas de AuthPolicies (internal en Biblioteca.API)
    private const string AdminOnly = "AdminOnly";
    private const string AdminOrLibrarian = "AdminOrLibrarian";
    private const string Authenticated = "Authenticated";

    [Theory]
    [InlineData(nameof(LibrosController.GetAllAsync), Authenticated)]
    [InlineData(nameof(LibrosController.GetByIdAsync), Authenticated)]
    [InlineData(nameof(LibrosController.CreateAsync), AdminOrLibrarian)]
    [InlineData(nameof(LibrosController.UpdateAsync), AdminOrLibrarian)]
    [InlineData(nameof(LibrosController.DeleteAsync), AdminOrLibrarian)]
    public void LibrosController_endpoints_tienen_la_politica_correcta(string methodName, string expectedPolicy)
    {
        var method = typeof(LibrosController).GetMethod(methodName);
        method.Should().NotBeNull();

        var authorize = method!
            .GetCustomAttributes<AuthorizeAttribute>(inherit: false)
            .FirstOrDefault();

        authorize.Should().NotBeNull($"{methodName} debe tener [Authorize]");
        authorize!.Policy.Should().Be(expectedPolicy,
            $"{methodName} debe requerir la política '{expectedPolicy}'");
    }

    [Theory]
    [InlineData(nameof(UsuariosController.GetAllAsync), AdminOnly)]
    [InlineData(nameof(UsuariosController.GetByIdAsync), AdminOnly)]
    [InlineData(nameof(UsuariosController.CreateAsync), AdminOnly)]
    [InlineData(nameof(UsuariosController.UpdateAsync), AdminOnly)]
    [InlineData(nameof(UsuariosController.DeleteAsync), AdminOnly)]
    public void UsuariosController_endpoints_tienen_la_politica_AdminOnly(string methodName, string expectedPolicy)
    {
        var method = typeof(UsuariosController).GetMethod(methodName);
        method.Should().NotBeNull();

        var authorize = method!
            .GetCustomAttributes<AuthorizeAttribute>(inherit: false)
            .FirstOrDefault();

        authorize.Should().NotBeNull($"{methodName} debe tener [Authorize]");
        authorize!.Policy.Should().Be(expectedPolicy,
            $"{methodName} debe requerir la política '{expectedPolicy}'");
    }
}
