using Application.DTO.Songs;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles;
internal class SongMappingProfile : Profile
{
    public SongMappingProfile()
    {
        CreateMap<Song, SongResponse>().ReverseMap();
        CreateMap<CreateSongRequest, Song>();
        CreateMap<UpdateSongRequest, Song>();
    }
}
