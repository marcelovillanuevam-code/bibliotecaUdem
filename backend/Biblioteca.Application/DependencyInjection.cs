using AutoMapper;
using Biblioteca.Application.Interfaces.Auth;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Application.Services.Auth;
using Biblioteca.Application.Services.Libros;
using Biblioteca.Application.Services.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace Biblioteca.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(DependencyInjection).Assembly);

        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILibroService, LibroService>();
        services.AddScoped<IBookCopyService, BookCopyService>();

        return services;
    }
}
