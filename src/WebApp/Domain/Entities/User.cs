using Domain.Primitives;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;
public class User : IdentityUser, ISoftDeletable
{
    public new string Email
    {
        get => base.Email ?? "";
        set => base.Email = value;
    }
    public string DisplayName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public List<string> Roles { get; set; } = [];
    public bool isDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }

    public Photo? Photo { get; set; }
    public List<Playlist> Playlists { get; } = [];
    public List<Song> Songs { get; } = [];
    public List<Song> LikedSongs { get; } = [];
    public List<Playlist> LikedPlaylists { get; } = [];
}
