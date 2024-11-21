﻿using Application.DTO.Genres;
using Application.DTO.Users;

namespace Application.DTO.Songs;
public class SongResponse
{
    public int SongId { get; set; }
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int LikesCount { get; set; } = default;
    public int Duration { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GenreResponse> Genres { get; } = [];
    public string? PhotoUrl { get; set; }
    public ArtistResponse Artist { get; set; } = null!;
}
