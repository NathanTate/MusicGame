using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Models.Queries;
public static class QueryExtensions
{
    public static Expression<Func<Playlist, object>> GetSortProperty(this PlaylistsQuery query) =>
        query.SortColumn?.ToLower() switch
        {
            "name" => playlist => playlist.Name,
            "likes" => playlist => playlist.LikesCount,
            "duration" => playlist => playlist.TotalDuration,
            "songsCount" => playlist => playlist.Songs.Count,
             _ => playlist => playlist.PlaylistId
        };

    public static Expression<Func<Song, object>> GetSortProperty(this SongsQuery query) =>
        query.SortColumn?.ToLower() switch
        {
            "name" => song => song.Name,
            "likes" => song => song.LikesCount,
            "duration" => song => song.Duration,
            "releaseDate" => song => song.ReleaseDate,
            "createdDate" => song => song.CreatedAt,
            _ => song => song.SongId
        };

    public static Expression<Func<Playlist, object>> GetSortProperty(this UserPlaylistsQuery query) =>
        query.SortColumn?.ToLower() switch
        {
            "name" => playlist => playlist.Name,
            "likes" => playlist => playlist.LikesCount,
            "duration" => playlist => playlist.TotalDuration,
            "songsCount" => playlist => playlist.Songs.Count,
            _ => playlist => playlist.PlaylistId
        };

    public static Expression<Func<Song, object>> GetSortProperty(this UserSongsQuery query) =>
        query.SortColumn?.ToLower() switch
        {
            "name" => song => song.Name,
            "likes" => song => song.LikesCount,
            "duration" => song => song.Duration,
            "releaseDate" => song => song.ReleaseDate,
            "createdDate" => song => song.CreatedAt,
            _ => song => song.SongId
        };
}
