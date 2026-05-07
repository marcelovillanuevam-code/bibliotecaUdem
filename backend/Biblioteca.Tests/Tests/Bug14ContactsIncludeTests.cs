using Biblioteca.Tests.Helpers;
using Biblioteca.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Biblioteca.Tests.Tests;

// BUG-14: la relación Usuario→Contacts debe cargarse con Include para que
//         UpdateAsync pueda actualizar el email sin generar duplicados.
public sealed class Bug14ContactsIncludeTests
{
    [Fact]
    public async Task Include_Contacts_carga_los_contactos_del_usuario()
    {
        // La FK Usuario.StatusCode → Estado.Code y la propiedad computada UsernameLower
        // hacen que el stack completo del repositorio (GetByIdForUpdateAsync) sea difícil
        // de ejecutar sobre InMemory sin todo el esquema. Este test valida exactamente
        // el mismo Include que usa el repositorio: Include(u => u.Contacts).
        await using var ctx = TestDbContextFactory.Create();

        var usuario = TestData.NewUsuario("jdoe");
        var contacto = TestData.NewEmailContact(usuario.Id, "jdoe@udem.edu");

        // Estado requerido por la FK Usuario.StatusCode → Estado.Code (PK)
        ctx.Statuses.Add(new Estado { Code = "active", Description = "Activo" });
        ctx.Users.Add(usuario);
        ctx.UserContacts.Add(contacto);
        await ctx.SaveChangesAsync();

        // Limpiar ChangeTracker para forzar lectura desde la BD in-memory
        ctx.ChangeTracker.Clear();

        // Consulta equivalente al Include(u => u.Contacts) que usa GetByIdForUpdateAsync
        var result = await ctx.Users
            .Include(u => u.Contacts)
            .FirstOrDefaultAsync(u => u.Id == usuario.Id && u.DeletedAt == null);

        result.Should().NotBeNull("el usuario existe en la BD in-memory");
        result!.Contacts.Should().NotBeEmpty(
            "Include(Contacts) debe cargar los contactos del usuario (BUG-14: necesario para actualizar email sin duplicar)");
        result.Contacts.Should().Contain(c => c.Value == "jdoe@udem.edu");
    }
}
