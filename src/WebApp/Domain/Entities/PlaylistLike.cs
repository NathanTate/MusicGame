namespace Domain.Entities;
public class PlaylistLike
{
    public int PlaylistId { get; set; }
    public Playlist Playlist { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
