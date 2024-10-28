namespace Domain.Entities;
public class Song
{
    public int SongId { get; set; }
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public int LikesCount { get; set; } = default;
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Genre> Genres { get; } = [];
    public List<Playlist> Playlists { get; } = [];
    public List<User> UserLikes { get; } = [];

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public int? PhotoId { get; set; }
    public Photo? Photo { get; set; }
}
