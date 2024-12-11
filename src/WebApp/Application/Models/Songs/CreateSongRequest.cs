﻿using Application.Models.Genres;
using Microsoft.AspNetCore.Http;

namespace Application.Models.Songs;

public sealed class CreateSongRequest
{
    public string Name { get; set; } = null!;
    public int Duration { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public IFormFile SongFile { get; set; } = null!;
    public List<int> GenreIds { get; set; } = [];
}
