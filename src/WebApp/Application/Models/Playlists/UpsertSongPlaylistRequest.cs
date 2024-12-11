using Microsoft.AspNetCore.Mvc;

namespace Application.Models.Playlists;

public sealed class UpsertSongPlaylistRequest
{
    [FromRoute]
    public int SongId { get; set; }

    [FromRoute]
    public int PlaylistId { get; set; }

    [FromQuery]
    public int Position { get; set; }
}


