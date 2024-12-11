using Application.Models.Songs;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles;
internal class SongMappingProfile : Profile
{
    public SongMappingProfile()
    {
        CreateMap<Song, SongResponse>()
            .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photo != null ? src.Photo.URL : ""));

        CreateMap<CreateSongRequest, Song>()
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.SongFile.Length))
            .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.SongFile.ContentType));

        CreateMap<UpdateSongRequest, Song>();
    }
}
