using Application.DTO.Genres;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.Songs;
public class UpdateSongRequest
{
    public int SongId { get; set; }
    public string Name { get; set; } = null!;
    public int Duration { get; set; }
    public IFormFile? PhotoFile { get; set; }
    public DateTime ReleaseDate { get; set; }
    public List<GenreResponse> Genres { get; } = [];
}
