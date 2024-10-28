namespace Domain.Entities;
public class Photo
{
    public int PhotoId { get; set; }
    public string URL { get; set; } = null!;
    public string PublicId { get; set; } = null!;

    public List<Song> Songs { get; } = [];
    public List<Playlist> Playlists { get; } = [];
}
