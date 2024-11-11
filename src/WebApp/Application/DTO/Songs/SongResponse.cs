using Application.DTO.Genres;

namespace Application.DTO.Songs;
public class SongResponse
{
    public string UserId { get; set; } = null!;
    public int SongId { get; set; }
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int LikesCount { get; set; } = default;
    public int Duration { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GenreResponse> Genres { get; } = [];
    public PhotoResponse? Photo { get; set; }
}
