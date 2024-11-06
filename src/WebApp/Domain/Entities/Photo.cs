using Domain.Primitives;

namespace Domain.Entities;
public class Photo : ISoftDeletable
{
    public int PhotoId { get; set; }
    public string URL { get; set; } = null!;
    public string PublicId { get; set; } = null!;
    public bool isDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }

    public List<Song> Songs { get; } = [];
    public List<Playlist> Playlists { get; } = [];
}
