namespace Domain.Entities;
public class SongLike
{
    public int SongId { get; set; }
    public Song Song { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
