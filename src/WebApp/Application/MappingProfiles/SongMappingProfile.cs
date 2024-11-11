using Application.DTO.Songs;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles;
internal class SongMappingProfile : Profile
{
    public SongMappingProfile()
    {
        CreateMap<Song, SongResponse>().ReverseMap();
        CreateMap<CreateSongRequest, Song>()
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.SongFile.Length))
            .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.SongFile.ContentType));
        CreateMap<UpdateSongRequest, Song>();
    }
}
