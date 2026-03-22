using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Application.Services.Libros;
using Biblioteca.Application.Services.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace Biblioteca.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ILibroService, LibroService>();

        return services;
    }
}
