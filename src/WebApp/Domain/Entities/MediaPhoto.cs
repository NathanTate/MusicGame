namespace Domain.Entities;
public class MediaPhoto : Photo
{
    public int MediaPhotoId { get; set; }
    public List<Song> Songs { get; } = [];
    public List<Playlist> Playlists { get; } = [];
}
