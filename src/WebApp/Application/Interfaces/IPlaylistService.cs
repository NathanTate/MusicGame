using Application.DTO.Playlists;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;
public interface IPlaylistService
{
    Task<Result> AddSongToPlaylistAsync(UpsertSongPlaylistRequest model, string userId, CancellationToken cancellationToken = default);
    Task<Result> RemoveSongFromPlaylistAsync(int songId, int playlistId, CancellationToken cancellationToken = default);
    Task<Result> UpdateSongPositionAsync(UpsertSongPlaylistRequest model, CancellationToken cancellationToken = default);
    Task<PlaylistResponse> CreatePlaylistAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<PlaylistResponse>> UpdatePlaylistAsync(UpdatePlaylistRequest model, CancellationToken cancellationToken = default);
    Task<Result> DeletePlaylistAsync(int playlistId, string userId, CancellationToken cancellationToken = default);
    Task<Result<PlaylistResponse>> GetPlaylistAsync(int playlistId, string? userId, CancellationToken cancellationToken = default);
    Task<List<PlaylistResponse>> GetPlaylistsAsync(CancellationToken cancellationToken = default);
    Task<Result<PlaylistResponse>> UploadPhotoAsync(int playlistId, IFormFile photo, CancellationToken cancellationToken = default);
    Task<Result> DeletePhotoAsync(int playlistId, CancellationToken cancellationToken = default);
}
