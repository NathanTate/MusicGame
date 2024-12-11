using Application.Models.Users;

namespace Application.Models.Playlists;
public sealed class PlaylistResponse
{
    public int PlaylistId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsPrivate { get; set; } = false;
    public int TotalDuration { get; set; }
    public int SongsCount { get; set; } = default;
    public int LikesCount { get; set; } = default;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<PlaylistSongResponse> Songs { get; set; } = [];
    public string? PhotoUrl { get; set; }
    public ArtistResponse User { get; set; } = null!;
}
