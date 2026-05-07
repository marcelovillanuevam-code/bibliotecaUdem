using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Loans;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Persistence.Context;
using Biblioteca.Persistence.Repositories;
using Biblioteca.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Biblioteca.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("The connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<BibliotecaDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ILibroRepository, LibroRepository>();
        services.AddScoped<IBookCopyRepository, BookCopyRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IDatabaseSeeder, StatusesSeeder>();
        services.AddScoped<IDatabaseSeeder, RolesSeeder>();
        services.AddScoped<IDatabaseSeeder, UsersSeeder>();
        services.AddScoped<IDatabaseSeeder, UserProfilesSeeder>();
        services.AddScoped<IDatabaseSeeder, UserAuthSeeder>();
        services.AddScoped<IDatabaseSeeder, UserContactsSeeder>();
        services.AddScoped<IDatabaseSeeder, UserRolesSeeder>();
        services.AddScoped<IDatabaseSeeder, AuthorsSeeder>();
        services.AddScoped<IDatabaseSeeder, SubjectsSeeder>();
        services.AddScoped<IDatabaseSeeder, BooksSeeder>();
        services.AddScoped<IDatabaseSeeder, BookAuthorsSeeder>();
        services.AddScoped<IDatabaseSeeder, BookSubjectsSeeder>();
        services.AddScoped<IDatabaseSeeder, LocationsSeeder>();
        services.AddScoped<IDatabaseSeeder, SessionsSeeder>();
        services.AddScoped<IDatabaseSeeder, PasswordResetTokensSeeder>();
        services.AddScoped<IDatabaseSeeder, AuditLogsSeeder>();

        return services;
    }
}
