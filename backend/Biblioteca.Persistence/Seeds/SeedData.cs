using Biblioteca.Domain.Entities;

namespace Biblioteca.Persistence.Seeds;

public static class SeedData
{
    public static readonly Usuario[] Usuarios =
    [
        new()
        {
            Id = Guid.Parse("2A0F1D20-88AC-4FE4-8B74-2A0B505D0001"),
            NombreCompleto = "Admin Biblioteca",
            Email = "admin@udem.edu.mx",
            Matricula = "A00000001",
            FechaRegistro = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            Activo = true
        },
        new()
        {
            Id = Guid.Parse("2A0F1D20-88AC-4FE4-8B74-2A0B505D0002"),
            NombreCompleto = "Mariana Garza",
            Email = "mariana.garza@udem.edu.mx",
            Matricula = "A00000002",
            FechaRegistro = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc),
            Activo = true
        }
    ];

    public static readonly Libro[] Libros =
    [
        new()
        {
            Id = Guid.Parse("4B10E420-77BD-4AE2-95A8-6A0B505D1001"),
            Titulo = "Clean Architecture",
            Autor = "Robert C. Martin",
            Isbn = "9780134494166",
            Stock = 3,
            FechaRegistro = new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc),
            Disponible = true
        },
        new()
        {
            Id = Guid.Parse("4B10E420-77BD-4AE2-95A8-6A0B505D1002"),
            Titulo = "Domain-Driven Design",
            Autor = "Eric Evans",
            Isbn = "9780321125217",
            Stock = 2,
            FechaRegistro = new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc),
            Disponible = true
        }
    ];
}
