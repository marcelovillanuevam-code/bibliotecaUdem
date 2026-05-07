using Biblioteca.API.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Xunit;

namespace Biblioteca.Tests.Tests;

// BUG-04: GlobalExceptionMiddleware no debe exponer exception.Message en respuestas 500
public sealed class Bug04MiddlewareTests
{
    [Fact]
    public async Task Error_500_no_expone_el_mensaje_interno_de_la_excepcion()
    {
        // Arrange: delegate que lanza un error inesperado con detalle sensible
        const string internalSecret = "connection string = Server=prod;Password=secret123";
        RequestDelegate next = _ => throw new InvalidOperationException(internalSecret);

        var logger = NullLogger<GlobalExceptionMiddleware>.Instance;
        var middleware = new GlobalExceptionMiddleware(next, logger);

        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Act
        await middleware.InvokeAsync(context);

        // Assert: status 500 y Detail genérico, nunca el mensaje interno
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        responseBody.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(responseBody).ReadToEndAsync();

        var problem = JsonSerializer.Deserialize<ProblemDetails>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        problem.Should().NotBeNull();
        problem!.Detail.Should().Be("Se produjo un error inesperado.",
            "el Detail del 500 debe ser genérico");
        problem.Detail.Should().NotContain(internalSecret,
            "el mensaje interno no debe exponerse al cliente");
    }

    [Fact]
    public async Task Error_400_expone_el_mensaje_de_validacion()
    {
        // Las excepciones conocidas (400/404/409/401) sí exponen su mensaje
        const string validationMessage = "El campo Email es requerido.";
        RequestDelegate next = _ =>
            throw new System.ComponentModel.DataAnnotations.ValidationException(validationMessage);

        var middleware = new GlobalExceptionMiddleware(next, NullLogger<GlobalExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        problem!.Detail.Should().Be(validationMessage);
    }
}
