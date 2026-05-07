using Biblioteca.Tests.Helpers;
using Biblioteca.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Biblioteca.Tests.Tests;

// BUG-02: Eliminar un libro debe hacer soft-delete (DeletedAt), no hard-delete
public sealed class Bug02SoftDeleteBookTests
{
    [Fact]
    public async Task DeleteAsync_marca_DeletedAt_y_el_libro_desaparece_del_catalogo()
    {
        var dbName = nameof(Bug02SoftDeleteBookTests);
        var libro = TestData.NewLibro("Cien años de soledad");

        // Arrange: guardar libro en BD in-memory
        await using (var ctx = TestDbContextFactory.Create(dbName))
        {
            ctx.Books.Add(libro);
            await ctx.SaveChangesAsync();
        }

        // Act: soft-delete vía repositorio
        await using (var ctx = TestDbContextFactory.Create(dbName))
        {
            var repo = new LibroRepository(ctx, new FixedDateTimeProvider());
            var tracked = await ctx.Books.IgnoreQueryFilters()
                .FirstAsync(b => b.Id == libro.Id);

            repo.Remove(tracked);
            await repo.SaveChangesAsync(CancellationToken.None);
        }

        // Assert: DeletedAt está seteado y el libro no aparece en el catálogo normal
        await using (var ctx = TestDbContextFactory.Create(dbName))
        {
            var deleted = await ctx.Books.IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == libro.Id);

            deleted.Should().NotBeNull();
            deleted!.DeletedAt.Should().Be(FixedDateTimeProvider.Fixed,
                "Remove() debe setear DeletedAt con la fecha del proveedor");

            var catalogCount = await ctx.Books.CountAsync();
            catalogCount.Should().Be(0, "el HasQueryFilter debe excluir libros soft-deleted");
        }
    }
}
