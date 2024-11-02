using Application.DTO.Genres;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles;
internal class GenreMappingProfile : Profile
{
    public GenreMappingProfile()
    {
        CreateMap<Genre, GenreResponse>().ReverseMap();
        CreateMap<CreateGenreRequest, Genre>();
        CreateMap<UpdateGenreRequest, Genre>(); 
    }
}
