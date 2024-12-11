using Application.Models.Playlists;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles;
internal class PlaylistMappingProfile : Profile
{
    public PlaylistMappingProfile()
    {
        CreateMap<Playlist, PlaylistResponse>()
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photo != null ? src.Photo.URL : ""))
            .ForMember(dest => dest.Songs, opt => opt.MapFrom(src => src.Songs.OrderBy(x => x.Position)));

        CreateMap<UpdatePlaylistRequest, Playlist>();
        CreateMap<UpsertSongPlaylistRequest, PlaylistSong>();
        CreateMap<PlaylistSong, PlaylistSongResponse>()
            .ForMember(dest => dest.Song, opt => opt.MapFrom(src => src.Song));
    }
}
