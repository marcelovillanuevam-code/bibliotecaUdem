using System.ComponentModel.DataAnnotations;
using Biblioteca.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Middlewares;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, "Solicitud invalida."),
                NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado."),
                ConflictException => (StatusCodes.Status409Conflict, "Conflicto de negocio."),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "No autorizado."),
                _ => (StatusCodes.Status500InternalServerError, "Se produjo un error inesperado.")
            };

            logger.LogError(exception, "Request failed with status code {StatusCode}.", statusCode);

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = title,
                Detail = statusCode == StatusCodes.Status500InternalServerError
                    ? "Se produjo un error inesperado."
                    : exception.Message,
                Status = statusCode,
                Instance = context.Request.Path
            });
        }
    }
}
