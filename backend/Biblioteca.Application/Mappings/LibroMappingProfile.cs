using AutoMapper;
using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Mappings;

public sealed class LibroMappingProfile : Profile
{
    public LibroMappingProfile()
    {
        CreateMap<LibroAutor, LibroAutorDto>()
            .ForCtorParam(nameof(LibroAutorDto.Id), opt => opt.MapFrom(src => src.AuthorId))
            .ForCtorParam(nameof(LibroAutorDto.FullName), opt => opt.MapFrom(src =>
                src.Author != null ? src.Author.FullName : string.Empty))
            .ForCtorParam(nameof(LibroAutorDto.Contribution), opt => opt.MapFrom(src => src.Contribution));

        CreateMap<LibroMateria, LibroMateriaDto>()
            .ForCtorParam(nameof(LibroMateriaDto.Id), opt => opt.MapFrom(src => src.SubjectId))
            .ForCtorParam(nameof(LibroMateriaDto.Code), opt => opt.MapFrom(src =>
                src.Subject != null ? src.Subject.Code : string.Empty))
            .ForCtorParam(nameof(LibroMateriaDto.Name), opt => opt.MapFrom(src =>
                src.Subject != null ? src.Subject.Name : string.Empty));

        CreateMap<Libro, LibroDto>()
            .ForCtorParam(nameof(LibroDto.Authors), opt => opt.MapFrom(src =>
                src.Authors
                    .OrderBy(author => author.Author != null ? author.Author.FullName : string.Empty)
                    .ToArray()))
            .ForCtorParam(nameof(LibroDto.Subjects), opt => opt.MapFrom(src =>
                src.Subjects
                    .OrderBy(subject => subject.Subject != null ? subject.Subject.Name : string.Empty)
                    .ToArray()));

        CreateMap<Libro, LibroFichaDto>()
            .ForCtorParam(nameof(LibroFichaDto.Authors), opt => opt.MapFrom(src =>
                src.Authors
                    .OrderBy(author => author.Author != null ? author.Author.FullName : string.Empty)
                    .ToArray()))
            .ForCtorParam(nameof(LibroFichaDto.Subjects), opt => opt.MapFrom(src =>
                src.Subjects
                    .OrderBy(subject => subject.Subject != null ? subject.Subject.Name : string.Empty)
                    .ToArray()));
    }
}
