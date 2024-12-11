using Application.Models.Genres;

namespace Application.Models.Songs;
public class UpdateSongRequest
{
    public int SongId { get; set; }
    public string Name { get; set; } = null!;
    public int Duration { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public List<int> GenreIds { get; set; } = [];
}
