using Microsoft.AspNetCore.Http;

namespace Application.DTO.Songs;
//public sealed record CreateSongRequest(int PhotoId, string Name, DateTime ReleaseDate, IFormFile SongFile, IFormFile? PhotoFile);

public sealed class CreateSongRequest
{
    public string Name { get; set; } = null!;
    public int Duration { get; set; }
    public DateTime ReleaseDate { get; set; }
    public IFormFile SongFile { get; set; } = null!;
    public IFormFile? PhotoFile { get; set; }
}

