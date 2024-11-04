using Application.DTO.Songs;
using FluentResults;

namespace Application.Interfaces;
public interface ISongService
{
    Task<List<SongResponse>> GetSongsAsync(CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> GetSongAsync(int songId, CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> CreateSongAsync(CreateSongRequest model, string userId, CancellationToken cancellationToken = default);
    Task<Result<SongResponse>> UpdateSongAsync(UpdateSongRequest model, CancellationToken cancellationToken = default);
    Task<Result> DeleteSongAsync(int songId, CancellationToken cancellationToken = default);
}
