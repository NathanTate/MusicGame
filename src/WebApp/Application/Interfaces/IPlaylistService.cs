using Application.DTO.Playlists;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;
public interface IPlaylistService
{
    Task<Result> CreatePlaylistAsync(CancellationToken cancellationToken = default);
    Task<Result> UpdatePlaylistAsync(UpdatePlaylistRequest model, CancellationToken cancellationToken = default);
    Task<Result> DeletePlaylistAsync(int playlistId, CancellationToken cancellationToken = default);
    Task<Result> GetPlaylistAsync(int playlistId, CancellationToken cancellationToken = default);
    Task<Result> GetPlaylistsAsync(CancellationToken cancellationToken = default);
    Task<Result<PlaylistResponse>> UploadPhotoAsync(int songId, IFormFile photo, CancellationToken cancellationToken = default);
    Task<Result> DeletePhotoAsync(int songId, CancellationToken cancellationToken = default);
}
