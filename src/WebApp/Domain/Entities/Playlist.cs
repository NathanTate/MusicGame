namespace Domain.Entities;
public class Playlist
{
    public int PlaylistId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsPrivate { get; set; } = false;
    public int TotalDuration { get; set; }
    public int SongsCount { get; set; } = default;
    public int LikesCount { get; set; } = default;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<User> UserLikes { get; } = [];
    public List<Song> Songs { get; } = [];

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public int? PhotoId { get; set; }
    public Photo? Photo { get; set; }
}
