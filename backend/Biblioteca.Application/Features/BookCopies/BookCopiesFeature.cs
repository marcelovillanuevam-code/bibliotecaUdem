namespace Biblioteca.Application.Features.BookCopies;

public static class BookCopiesFeature
{
    public const string ByBookRoute = "api/libros/{libroId:guid}/ejemplares";
    public const string BaseRoute = "api/ejemplares";
}
