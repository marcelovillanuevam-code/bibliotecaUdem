using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Seeding;

internal static class SeedData
{
    public static readonly DateTime UserSeedTimestamp = new(2026, 3, 19, 18, 20, 25, DateTimeKind.Utc);
    public static readonly DateTime CatalogSeedTimestamp = new(2026, 3, 19, 18, 28, 31, DateTimeKind.Utc);
    public static readonly DateTime SessionSeedTimestamp = new(2026, 3, 22, 23, 15, 0, DateTimeKind.Utc);
    public static readonly DateTime PasswordResetSeedTimestamp = new(2026, 3, 22, 22, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime AuditSeedTimestamp = new(2026, 3, 22, 23, 20, 0, DateTimeKind.Utc);

    public static readonly Guid StudentRoleId = Guid.Parse("5ba2f895-23c0-11f1-b6ca-1cce51c6125a");
    public static readonly Guid LibrarianRoleId = Guid.Parse("5ba2ff3e-23c0-11f1-b6ca-1cce51c6125a");
    public static readonly Guid TeacherRoleId = Guid.Parse("5ba30106-23c0-11f1-b6ca-1cce51c6125a");
    public static readonly Guid AdminRoleId = Guid.Parse("5ba30193-23c0-11f1-b6ca-1cce51c6125a");

    public static readonly Guid AdminUserId = Guid.Parse("5ba80dcd-23c0-11f1-b6ca-1cce51c6125a");
    public static readonly Guid MariaUserId = Guid.Parse("1fd476e7-98be-494c-b0cb-598ba4f1ee01");
    public static readonly Guid LuisUserId = Guid.Parse("ee38d40e-b1d8-4486-831d-46ef4cf721ca");
    public static readonly Guid AnaUserId = Guid.Parse("b703e900-1231-49d7-b1bf-7f97a4cc5ce9");
    public static readonly Guid FernandaUserId = Guid.Parse("ab15755f-6591-41ff-85bc-f0a3b242fbd3");

    public static readonly Guid AdminContactId = Guid.Parse("5bbacf98-23c0-11f1-b6ca-1cce51c6125a");
    public static readonly Guid MariaContactId = Guid.Parse("2256f88d-bdd7-443d-84d8-b9c52f832f4e");
    public static readonly Guid LuisContactId = Guid.Parse("4d27284b-87b6-4af7-bccb-f9cf20f22f84");
    public static readonly Guid AnaContactId = Guid.Parse("4f21cb6f-fd02-4fd1-ab9c-165efad8fcbf");
    public static readonly Guid FernandaContactId = Guid.Parse("34a730a5-1202-4ef3-891e-7f73a6529d26");

    public static readonly Guid GabrielAuthorId = Guid.Parse("7d235a4e-23c1-11f1-b6ca-1cce51c6125a");
    public static readonly Guid IsaacAuthorId = Guid.Parse("7d23604a-23c1-11f1-b6ca-1cce51c6125a");
    public static readonly Guid JaneAuthorId = Guid.Parse("7d23d4b8-23c1-11f1-b6ca-1cce51c6125a");

    public static readonly Guid LitLatamSubjectId = Guid.Parse("7d24c19b-23c1-11f1-b6ca-1cce51c6125a");
    public static readonly Guid SciFiSubjectId = Guid.Parse("7d24c4ff-23c1-11f1-b6ca-1cce51c6125a");
    public static readonly Guid LitClassicSubjectId = Guid.Parse("7d24c7c0-23c1-11f1-b6ca-1cce51c6125a");

    public static readonly Guid CienAniosBookId = Guid.Parse("7d287354-23c1-11f1-b6ca-1cce51c6125a");
    public static readonly Guid FundacionBookId = Guid.Parse("7d288140-23c1-11f1-b6ca-1cce51c6125a");
    public static readonly Guid PrideBookId = Guid.Parse("7d288711-23c1-11f1-b6ca-1cce51c6125a");

    public static readonly Guid HumanidadesLocationId = Guid.Parse("7d25b5aa-23c1-11f1-b6ca-1cce51c6125a");
    public static readonly Guid CienciasLocationId = Guid.Parse("7d25bc4e-23c1-11f1-b6ca-1cce51c6125a");

    public static readonly Guid SessionId = Guid.Parse("2f4df84f-1655-4c57-9a65-1009a9859813");
    public static readonly Guid PasswordResetTokenId = Guid.Parse("df4c7495-ae00-4d53-afef-483ba0b5ac65");
    public static readonly Guid AuditLogId = Guid.Parse("d9f9ecde-b523-4146-8ee2-b823462eb4d1");
}

internal sealed class StatusesSeeder : IDatabaseSeeder
{
    public string TableName => "statuses";
    public int Order => 10;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new Estado { Code = "active", Description = "Activo" },
            new Estado { Code = "deleted", Description = "Eliminado" },
            new Estado { Code = "inactive", Description = "Inactivo" },
            new Estado { Code = "pending_verification", Description = "Pendiente de verificacion" },
            new Estado { Code = "suspended", Description = "Suspendido" }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.Statuses.FindAsync([entry.Code], cancellationToken);
            if (existing is null)
            {
                dbContext.Statuses.Add(entry);
                continue;
            }

            existing.Description = entry.Description;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class RolesSeeder : IDatabaseSeeder
{
    public string TableName => "roles";
    public int Order => 20;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new Rol { Id = SeedData.StudentRoleId, Code = "STUDENT", DisplayName = "Student", Description = "Estudiante", CreatedAt = SeedData.UserSeedTimestamp },
            new Rol { Id = SeedData.LibrarianRoleId, Code = "LIBRARIAN", DisplayName = "Librarian", Description = "Bibliotecario", CreatedAt = SeedData.UserSeedTimestamp },
            new Rol { Id = SeedData.TeacherRoleId, Code = "TEACHER", DisplayName = "Teacher", Description = "Profesor", CreatedAt = SeedData.UserSeedTimestamp },
            new Rol { Id = SeedData.AdminRoleId, Code = "ADMIN", DisplayName = "Administrator", Description = "Administrador", CreatedAt = SeedData.UserSeedTimestamp }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.Roles.FirstOrDefaultAsync(role => role.Id == entry.Id, cancellationToken);
            if (existing is null)
            {
                dbContext.Roles.Add(entry);
                continue;
            }

            existing.Code = entry.Code;
            existing.DisplayName = entry.DisplayName;
            existing.Description = entry.Description;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class UsersSeeder : IDatabaseSeeder
{
    public string TableName => "users";
    public int Order => 30;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new Usuario { Id = SeedData.AdminUserId, Username = "admin1", StatusCode = "active", PreferredLocale = "es_MX", CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new Usuario { Id = SeedData.MariaUserId, Username = "demo.mlopez", StatusCode = "active", PreferredLocale = "es_MX", CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new Usuario { Id = SeedData.LuisUserId, Username = "demo.lramirez", StatusCode = "active", PreferredLocale = "es_MX", CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new Usuario { Id = SeedData.AnaUserId, Username = "demo.amartinez", StatusCode = "active", PreferredLocale = "es_MX", CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new Usuario { Id = SeedData.FernandaUserId, Username = "demo.fsanchez", StatusCode = "inactive", PreferredLocale = "es_MX", CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == entry.Id, cancellationToken);
            if (existing is null)
            {
                dbContext.Users.Add(entry);
                continue;
            }

            existing.Username = entry.Username;
            existing.StatusCode = entry.StatusCode;
            existing.PreferredLocale = entry.PreferredLocale;
            existing.MetadataJson = null;
            existing.DeletedAt = null;
            existing.UpdatedAt = entry.UpdatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class UserProfilesSeeder : IDatabaseSeeder
{
    public string TableName => "user_profiles";
    public int Order => 40;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new PerfilUsuario { UserId = SeedData.AdminUserId, FirstName = "Carlos", LastName = "Garcia Mendoza", DisplayName = "Carlos Garcia Mendoza", DocumentType = "employee_id", DocumentNumber = "ADM-2024-001", CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new PerfilUsuario { UserId = SeedData.MariaUserId, FirstName = "Maria", LastName = "Lopez Torres", DisplayName = "Dra. Maria Lopez Torres", DocumentType = "employee_id", DocumentNumber = "PRF-2024-042", CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new PerfilUsuario { UserId = SeedData.LuisUserId, FirstName = "Luis", LastName = "Ramirez Perez", DisplayName = "Luis Ramirez Perez", DocumentType = "student_id", DocumentNumber = "EST-2024-198", CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new PerfilUsuario { UserId = SeedData.AnaUserId, FirstName = "Ana Sofia", LastName = "Martinez Ruiz", DisplayName = "Ana Sofia Martinez Ruiz", DocumentType = "student_id", DocumentNumber = "EST-2024-219", CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new PerfilUsuario { UserId = SeedData.FernandaUserId, FirstName = "Fernanda", LastName = "Sanchez Ibarra", DisplayName = "Fernanda Sanchez Ibarra", DocumentType = "employee_id", DocumentNumber = "LIB-2024-010", CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.UserProfiles.FindAsync([entry.UserId], cancellationToken);
            if (existing is null)
            {
                dbContext.UserProfiles.Add(entry);
                continue;
            }

            existing.FirstName = entry.FirstName;
            existing.LastName = entry.LastName;
            existing.DisplayName = entry.DisplayName;
            existing.DocumentType = entry.DocumentType;
            existing.DocumentNumber = entry.DocumentNumber;
            existing.UpdatedAt = entry.UpdatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class UserAuthSeeder(IPasswordHasher<Usuario> passwordHasher) : IDatabaseSeeder
{
    public string TableName => "user_auth";
    public int Order => 50;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .Where(user =>
                user.Id == SeedData.AdminUserId ||
                user.Id == SeedData.MariaUserId ||
                user.Id == SeedData.LuisUserId ||
                user.Id == SeedData.AnaUserId ||
                user.Id == SeedData.FernandaUserId)
            .ToDictionaryAsync(user => user.Id, cancellationToken);

        var entries = new[]
        {
            new { UserId = SeedData.AdminUserId, Password = "Admin123!" },
            new { UserId = SeedData.MariaUserId, Password = "Password123!" },
            new { UserId = SeedData.LuisUserId, Password = "Password123!" },
            new { UserId = SeedData.AnaUserId, Password = "Password123!" },
            new { UserId = SeedData.FernandaUserId, Password = "Password123!" }
        };

        foreach (var entry in entries)
        {
            if (!users.TryGetValue(entry.UserId, out var user))
            {
                continue;
            }

            var existing = await dbContext.UserAuth.FindAsync([entry.UserId], cancellationToken);
            if (existing is null)
            {
                dbContext.UserAuth.Add(new AutenticacionUsuario
                {
                    UserId = entry.UserId,
                    PasswordHash = passwordHasher.HashPassword(user, entry.Password),
                    PasswordChangedAt = SeedData.UserSeedTimestamp,
                    FailedLoginCount = 0,
                    LockedUntil = null,
                    CreatedAt = SeedData.UserSeedTimestamp,
                    UpdatedAt = SeedData.UserSeedTimestamp
                });

                continue;
            }

            existing.PasswordHash = passwordHasher.HashPassword(user, entry.Password);
            existing.PasswordChangedAt = SeedData.UserSeedTimestamp;
            existing.FailedLoginCount = 0;
            existing.LockedUntil = null;
            existing.MfaEnabled = false;
            existing.MfaSecret = null;
            existing.UpdatedAt = SeedData.UserSeedTimestamp;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class UserContactsSeeder : IDatabaseSeeder
{
    public string TableName => "user_contacts";
    public int Order => 60;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new ContactoUsuario { Id = SeedData.AdminContactId, UserId = SeedData.AdminUserId, Type = "email", Value = "admin@udem.edu", IsPrimary = true, IsVerified = true, VerifiedAt = SeedData.UserSeedTimestamp, CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new ContactoUsuario { Id = SeedData.MariaContactId, UserId = SeedData.MariaUserId, Type = "email", Value = "m.lopez.demo@udem.edu", IsPrimary = true, IsVerified = true, VerifiedAt = SeedData.UserSeedTimestamp, CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new ContactoUsuario { Id = SeedData.LuisContactId, UserId = SeedData.LuisUserId, Type = "email", Value = "l.ramirez.demo@udem.edu", IsPrimary = true, IsVerified = true, VerifiedAt = SeedData.UserSeedTimestamp, CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new ContactoUsuario { Id = SeedData.AnaContactId, UserId = SeedData.AnaUserId, Type = "email", Value = "a.martinez.demo@udem.edu", IsPrimary = true, IsVerified = true, VerifiedAt = SeedData.UserSeedTimestamp, CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp },
            new ContactoUsuario { Id = SeedData.FernandaContactId, UserId = SeedData.FernandaUserId, Type = "email", Value = "f.sanchez.demo@udem.edu", IsPrimary = true, IsVerified = true, VerifiedAt = SeedData.UserSeedTimestamp, CreatedAt = SeedData.UserSeedTimestamp, UpdatedAt = SeedData.UserSeedTimestamp }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.UserContacts.FindAsync([entry.Id], cancellationToken);
            if (existing is null)
            {
                dbContext.UserContacts.Add(entry);
                continue;
            }

            existing.Type = entry.Type;
            existing.Value = entry.Value;
            existing.IsPrimary = entry.IsPrimary;
            existing.IsVerified = entry.IsVerified;
            existing.VerifiedAt = entry.VerifiedAt;
            existing.UpdatedAt = entry.UpdatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class UserRolesSeeder : IDatabaseSeeder
{
    public string TableName => "user_roles";
    public int Order => 70;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new UsuarioRol { UserId = SeedData.AdminUserId, RoleId = SeedData.AdminRoleId, AssignedAt = SeedData.UserSeedTimestamp },
            new UsuarioRol { UserId = SeedData.MariaUserId, RoleId = SeedData.TeacherRoleId, AssignedAt = SeedData.UserSeedTimestamp },
            new UsuarioRol { UserId = SeedData.LuisUserId, RoleId = SeedData.StudentRoleId, AssignedAt = SeedData.UserSeedTimestamp },
            new UsuarioRol { UserId = SeedData.AnaUserId, RoleId = SeedData.StudentRoleId, AssignedAt = SeedData.UserSeedTimestamp },
            new UsuarioRol { UserId = SeedData.FernandaUserId, RoleId = SeedData.LibrarianRoleId, AssignedAt = SeedData.UserSeedTimestamp }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.UserRoles.FindAsync([entry.UserId, entry.RoleId], cancellationToken);
            if (existing is null)
            {
                dbContext.UserRoles.Add(entry);
                continue;
            }

            existing.AssignedAt = entry.AssignedAt;
            existing.AssignedBy = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class AuthorsSeeder : IDatabaseSeeder
{
    public string TableName => "authors";
    public int Order => 80;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new Autor { Id = SeedData.GabrielAuthorId, FullName = "Gabriel Garcia Marquez", BirthDate = new DateOnly(1927, 3, 6), BioJson = "{\"notes\":\"Realismo magico\"}", CreatedAt = SeedData.CatalogSeedTimestamp, UpdatedAt = SeedData.CatalogSeedTimestamp },
            new Autor { Id = SeedData.IsaacAuthorId, FullName = "Isaac Asimov", BirthDate = new DateOnly(1920, 1, 2), BioJson = "{\"notes\":\"Ciencia ficcion\"}", CreatedAt = SeedData.CatalogSeedTimestamp, UpdatedAt = SeedData.CatalogSeedTimestamp },
            new Autor { Id = SeedData.JaneAuthorId, FullName = "Jane Austen", BirthDate = new DateOnly(1775, 12, 16), BioJson = "{\"notes\":\"Novela clasica\"}", CreatedAt = SeedData.CatalogSeedTimestamp, UpdatedAt = SeedData.CatalogSeedTimestamp }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.Authors.FindAsync([entry.Id], cancellationToken);
            if (existing is null)
            {
                dbContext.Authors.Add(entry);
                continue;
            }

            existing.FullName = entry.FullName;
            existing.BirthDate = entry.BirthDate;
            existing.BioJson = entry.BioJson;
            existing.UpdatedAt = entry.UpdatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class SubjectsSeeder : IDatabaseSeeder
{
    public string TableName => "subjects";
    public int Order => 90;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new Materia { Id = SeedData.LitLatamSubjectId, Code = "LIT_LATAM", Name = "Literatura Latinoamericana", Description = "Obras de autores latinoamericanos", CreatedAt = SeedData.CatalogSeedTimestamp },
            new Materia { Id = SeedData.SciFiSubjectId, Code = "SCI_FI", Name = "Ciencia Ficcion", Description = "Obras de ciencia ficcion", CreatedAt = SeedData.CatalogSeedTimestamp },
            new Materia { Id = SeedData.LitClassicSubjectId, Code = "LIT_CLASSIC", Name = "Literatura Clasica", Description = "Obras clasicas de la literatura", CreatedAt = SeedData.CatalogSeedTimestamp }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.Subjects.FindAsync([entry.Id], cancellationToken);
            if (existing is null)
            {
                dbContext.Subjects.Add(entry);
                continue;
            }

            existing.Code = entry.Code;
            existing.Name = entry.Name;
            existing.Description = entry.Description;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class BooksSeeder : IDatabaseSeeder
{
    public string TableName => "books";
    public int Order => 100;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new Libro { Id = SeedData.CienAniosBookId, Title = "Cien anos de soledad", Isbn = "9780307474728", Publisher = "Editorial Sudamericana", PublicationYear = 1967, Edition = "1", Language = "es", SummaryJson = "{\"short\":\"Saga familiar en Macondo\"}", MetadataJson = "{\"format\":\"tapa dura\"}", CreatedAt = SeedData.CatalogSeedTimestamp, UpdatedAt = SeedData.CatalogSeedTimestamp },
            new Libro { Id = SeedData.FundacionBookId, Title = "Fundacion", Isbn = "9780553293357", Publisher = "Gnome Press", PublicationYear = 1951, Edition = "1", Language = "es", SummaryJson = "{\"short\":\"Saga galactica sobre psicohistoria\"}", MetadataJson = "{\"format\":\"tapa blanda\"}", CreatedAt = SeedData.CatalogSeedTimestamp, UpdatedAt = SeedData.CatalogSeedTimestamp },
            new Libro { Id = SeedData.PrideBookId, Title = "Pride and Prejudice", Subtitle = "A Novel", Isbn = "9780141199078", Publisher = "T. Egerton", PublicationYear = 1813, Edition = "1", Language = "en", SummaryJson = "{\"short\":\"Novela sobre costumbres y matrimonio\"}", MetadataJson = "{\"format\":\"tapa blanda\"}", CreatedAt = SeedData.CatalogSeedTimestamp, UpdatedAt = SeedData.CatalogSeedTimestamp }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.Books
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(book => book.Id == entry.Id, cancellationToken);

            if (existing is null)
            {
                dbContext.Books.Add(entry);
                continue;
            }

            existing.DeletedAt = null;
            existing.Title = entry.Title;
            existing.Subtitle = entry.Subtitle;
            existing.Isbn = entry.Isbn;
            existing.Publisher = entry.Publisher;
            existing.PublicationYear = entry.PublicationYear;
            existing.Edition = entry.Edition;
            existing.Language = entry.Language;
            existing.SummaryJson = entry.SummaryJson;
            existing.MetadataJson = entry.MetadataJson;
            existing.UpdatedAt = entry.UpdatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class BookAuthorsSeeder : IDatabaseSeeder
{
    public string TableName => "book_authors";
    public int Order => 110;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new LibroAutor { BookId = SeedData.CienAniosBookId, AuthorId = SeedData.GabrielAuthorId, Contribution = "Autor" },
            new LibroAutor { BookId = SeedData.FundacionBookId, AuthorId = SeedData.IsaacAuthorId, Contribution = "Autor" },
            new LibroAutor { BookId = SeedData.PrideBookId, AuthorId = SeedData.JaneAuthorId, Contribution = "Autor" }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.BookAuthors.FindAsync([entry.BookId, entry.AuthorId], cancellationToken);
            if (existing is null)
            {
                dbContext.BookAuthors.Add(entry);
                continue;
            }

            existing.Contribution = entry.Contribution;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class BookSubjectsSeeder : IDatabaseSeeder
{
    public string TableName => "book_subjects";
    public int Order => 120;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new LibroMateria { BookId = SeedData.CienAniosBookId, SubjectId = SeedData.LitLatamSubjectId },
            new LibroMateria { BookId = SeedData.FundacionBookId, SubjectId = SeedData.SciFiSubjectId },
            new LibroMateria { BookId = SeedData.PrideBookId, SubjectId = SeedData.LitClassicSubjectId }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.BookSubjects.FindAsync([entry.BookId, entry.SubjectId], cancellationToken);
            if (existing is null)
            {
                dbContext.BookSubjects.Add(entry);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class LocationsSeeder : IDatabaseSeeder
{
    public string TableName => "locations";
    public int Order => 130;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var entries = new[]
        {
            new Ubicacion { Id = SeedData.HumanidadesLocationId, LibraryName = "Biblioteca Central", Section = "Humanidades", Shelf = "A1", Notes = "Estanteria principal Humanidades", CreatedAt = SeedData.CatalogSeedTimestamp },
            new Ubicacion { Id = SeedData.CienciasLocationId, LibraryName = "Biblioteca Central", Section = "Ciencias", Shelf = "C3", Notes = "Estanteria Ciencia y Tecnologia", CreatedAt = SeedData.CatalogSeedTimestamp }
        };

        foreach (var entry in entries)
        {
            var existing = await dbContext.Locations.FindAsync([entry.Id], cancellationToken);
            if (existing is null)
            {
                dbContext.Locations.Add(entry);
                continue;
            }

            existing.LibraryName = entry.LibraryName;
            existing.Section = entry.Section;
            existing.Shelf = entry.Shelf;
            existing.Notes = entry.Notes;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class SessionsSeeder : IDatabaseSeeder
{
    public string TableName => "sessions";
    public int Order => 140;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Sessions.FindAsync([SeedData.SessionId], cancellationToken);
        if (existing is null)
        {
            dbContext.Sessions.Add(new Sesion
            {
                Id = SeedData.SessionId,
                UserId = SeedData.AdminUserId,
                SessionToken = "seeded-demo-session-token",
                IpAddress = "127.0.0.1",
                UserAgent = "Seeder/1.0",
                CreatedAt = SeedData.SessionSeedTimestamp,
                ExpiresAt = SeedData.SessionSeedTimestamp.AddHours(8)
            });
        }
        else
        {
            existing.UserId = SeedData.AdminUserId;
            existing.SessionToken = "seeded-demo-session-token";
            existing.IpAddress = "127.0.0.1";
            existing.UserAgent = "Seeder/1.0";
            existing.CreatedAt = SeedData.SessionSeedTimestamp;
            existing.ExpiresAt = SeedData.SessionSeedTimestamp.AddHours(8);
            existing.RevokedAt = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class PasswordResetTokensSeeder : IDatabaseSeeder
{
    public string TableName => "password_reset_tokens";
    public int Order => 150;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var existing = await dbContext.PasswordResetTokens.FindAsync([SeedData.PasswordResetTokenId], cancellationToken);
        if (existing is null)
        {
            dbContext.PasswordResetTokens.Add(new TokenRestablecimientoContrasena
            {
                Id = SeedData.PasswordResetTokenId,
                UserId = SeedData.AdminUserId,
                Token = "seeded-demo-reset-token",
                CreatedAt = SeedData.PasswordResetSeedTimestamp,
                ExpiresAt = SeedData.PasswordResetSeedTimestamp.AddHours(2)
            });
        }
        else
        {
            existing.UserId = SeedData.AdminUserId;
            existing.Token = "seeded-demo-reset-token";
            existing.CreatedAt = SeedData.PasswordResetSeedTimestamp;
            existing.ExpiresAt = SeedData.PasswordResetSeedTimestamp.AddHours(2);
            existing.UsedAt = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class AuditLogsSeeder : IDatabaseSeeder
{
    public string TableName => "audit_logs";
    public int Order => 160;

    public async Task SeedAsync(BibliotecaDbContext dbContext, CancellationToken cancellationToken)
    {
        var existing = await dbContext.AuditLogs.FindAsync([SeedData.AuditLogId], cancellationToken);
        if (existing is null)
        {
            dbContext.AuditLogs.Add(new RegistroAuditoria
            {
                Id = SeedData.AuditLogId,
                TableName = "users",
                RecordId = SeedData.AdminUserId,
                Action = "INSERT",
                PerformedBy = SeedData.AdminUserId,
                PerformedAt = SeedData.AuditSeedTimestamp,
                ChangedDataJson = "{\"seed\":\"initial-admin\"}",
                Reason = "Initial seed data"
            });
        }
        else
        {
            existing.TableName = "users";
            existing.RecordId = SeedData.AdminUserId;
            existing.Action = "INSERT";
            existing.PerformedBy = SeedData.AdminUserId;
            existing.PerformedAt = SeedData.AuditSeedTimestamp;
            existing.ChangedDataJson = "{\"seed\":\"initial-admin\"}";
            existing.Reason = "Initial seed data";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
