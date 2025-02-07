using Application.Models.Genres;
using Application.Models.Playlists;
using Application.Models.Songs;
using Application.Models.Users;

namespace Application.Models;
public class SearchData
{
    public string BestFitType { get; set; } = "";
    public object? BestFitItem { get; set; } = null;
    public PagedList<SongResponse> Songs { get; set; } = null!;
    public PagedList<PlaylistResponse> Playlists { get; set; } = null!;
    public PagedList<GenreResponse> Genres { get; set; } = null!;
    public PagedList<UserResponse> Users { get; set; } = null!;
    public bool HasItems { get; set; } = false;
}
