using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;
public class User : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public List<Playlist> Playlists { get; } = [];
    public List<Song> Songs { get; } = [];
    public List<Song> LikedSongs { get; } = [];
    public List<Playlist> LikedPlaylists { get; } = [];
}
