using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Biblioteca.Persistence.Seeding;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Biblioteca.Persistence.DatabaseInitialization");
        var dbContext = scope.ServiceProvider.GetRequiredService<BibliotecaDbContext>();
        var seeders = scope.ServiceProvider
            .GetServices<IDatabaseSeeder>()
            .OrderBy(seeder => seeder.Order)
            .ToArray();

        logger.LogInformation("Applying pending database migrations.");
        await dbContext.Database.MigrateAsync(cancellationToken);

        foreach (var seeder in seeders)
        {
            logger.LogInformation("Seeding table {TableName}.", seeder.TableName);
            await seeder.SeedAsync(dbContext, cancellationToken);
        }

        logger.LogInformation("Database initialization finished.");
    }
}
