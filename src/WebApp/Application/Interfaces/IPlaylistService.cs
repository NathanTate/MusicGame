using Application.Models;
using Application.Models.Playlists;
using Application.Models.Queries;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;
public interface IPlaylistService
{
    Task<Result> AddSongToPlaylistAsync(UpsertSongPlaylistRequest model, CancellationToken cancellationToken = default);
    Task<Result> RemoveSongFromPlaylistAsync(int songId, int playlistId, CancellationToken cancellationToken = default);
    Task<Result> UpdateSongPositionAsync(UpsertSongPlaylistRequest model, CancellationToken cancellationToken = default);
    Task<Result<PlaylistResponse>> CreatePlaylistAsync(CancellationToken cancellationToken = default);
    Task<Result<PlaylistResponse>> UpdatePlaylistAsync(UpdatePlaylistRequest model, CancellationToken cancellationToken = default);
    Task<Result> DeletePlaylistAsync(int playlistId, CancellationToken cancellationToken = default);
    Task<Result<PlaylistResponse>> GetPlaylistAsync(int playlistId, CancellationToken cancellationToken = default);
    Task<PagedList<PlaylistResponse>> GetPlaylistsAsync(PlaylistsQueryRequest query, CancellationToken cancellationToken = default);
    Task<Result<PlaylistResponse>> UploadPhotoAsync(int playlistId, IFormFile photo, CancellationToken cancellationToken = default);
    Task<Result> DeletePhotoAsync(int playlistId, CancellationToken cancellationToken = default);
    Task<bool> IsPlaylistNameAvailable(string name, CancellationToken cancellationToken = default);
}
