using Application.Models;
using Application.Models.Queries;
using Application.Models.Songs;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;
public interface ISongService
{
    Task<PagedList<SongResponse>> GetSongsAsync(SongsQuery query, CancellationToken cancellationToken = default);
    Task<PagedList<SongResponse>> GetByIdsAsync(IEnumerable<int> ids, BaseQuery query, CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> GetSongAsync(int songId, CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> CreateSongAsync(CreateSongRequest model, CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> UpdateSongAsync(UpdateSongRequest model, CancellationToken cancellationToken = default);
    Task<Result> DeleteSongAsync(int songId, CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> UploadPhotoAsync(int songId, IFormFile photo, CancellationToken cancellationToken = default);
    Task<Result> DeletePhotoAsync(int songId, CancellationToken cancellationToken = default);
    Task<bool> IsSongNameAvailableAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<bool>> ToggleLikeAsync(int songId, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsLiked(int songId, CancellationToken cancellationToken = default);
}
