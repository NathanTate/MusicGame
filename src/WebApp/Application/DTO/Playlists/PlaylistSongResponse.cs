using Application.DTO.Songs;

namespace Application.DTO.Playlists;
public sealed record PlaylistSongResponse(int Position, SongResponse Song);

