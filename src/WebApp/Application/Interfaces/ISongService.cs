using Application.DTO.Songs;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;
public interface ISongService
{
    Task<List<SongResponse>> GetSongsAsync(CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> GetSongAsync(int songId, string? userId, CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> CreateSongAsync(CreateSongRequest model, string userId, CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> UpdateSongAsync(UpdateSongRequest model, CancellationToken cancellationToken = default);
    Task<Result> DeleteSongAsync(int songId, CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> UploadPhotoAsync(int songId, IFormFile photo, CancellationToken cancellationToken = default);
    Task<Result> DeletePhotoAsync(int songId, CancellationToken cancellationToken = default);
    Task<bool> IsSongNameAvailableAsync(string name, CancellationToken cancellationToken = default);
}
