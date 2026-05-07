using Biblioteca.Persistence.Context;

namespace Biblioteca.Persistence.Seeding;

public interface IDatabaseSeeder
{
    string TableName { get; }
    int Order { get; }
    Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken);
}
