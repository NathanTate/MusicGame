using Application.Models.Songs;

namespace Application.Models.Playlists;
public sealed record PlaylistSongResponse(int Position, SongResponse Song);

