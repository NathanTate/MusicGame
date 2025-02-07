using Domain.Primitives;

namespace Domain.Entities;
public class Song : ISoftDeletable
{
    public int SongId { get; set; }
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int LikesCount { get; set; } = default;
    public int Duration { get; set; }
    public long Size { get; set; }
    public string ContentType { get; set; } = null!;
    public bool IsPrivate { get; set; } = false;
    public DateTime ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool isDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }

    public List<Genre> Genres { get; } = [];
    public List<PlaylistSong> Playlists { get; } = [];
    public List<SongLike> UserLikes { get; } = [];

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public int? PhotoId { get; set; }
    public Photo? Photo { get; set; }
}
