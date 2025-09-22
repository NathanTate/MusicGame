using Application.Models.Elastic;
using Application.Models.Genres;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles;
internal class GenreMappingProfile : Profile
{
    public GenreMappingProfile()
    {
        CreateMap<GenreResponse, Genre>().ReverseMap();
        CreateMap<CreateGenreRequest, Genre>();
        CreateMap<UpdateGenreRequest, Genre>(); 

        CreateMap<Genre, GenreDoc>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GenreId));
    }
}
