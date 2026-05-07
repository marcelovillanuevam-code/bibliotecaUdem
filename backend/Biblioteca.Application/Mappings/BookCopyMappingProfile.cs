using AutoMapper;
using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Mappings;

public sealed class BookCopyMappingProfile : Profile
{
    public BookCopyMappingProfile()
    {
        CreateMap<BookCopy, BookCopyDto>()
            .ForCtorParam(nameof(BookCopyDto.LocationName), opt => opt.MapFrom(src =>
                src.Location != null ? src.Location.LibraryName : null));
    }
}
