using Application.DTO.Genres;
using FluentResults;

namespace Application.Interfaces;
public interface IGenreService
{
    Task<List<GenreResponse>> GetGenresAsync(CancellationToken cancellationToken = default);
    Task<Result<GenreResponse>> GetGenreAsync(int genreId, CancellationToken cancellationToken = default);
    Task<Result<GenreResponse>> CreateGenreAsync(CreateGenreRequest model, CancellationToken cancellationToken = default);
    Task<Result<GenreResponse>> UpdateGenreAsync(UpdateGenreRequest model, CancellationToken cancellationToken = default);
    Task<Result> DeleteGenreAsync(int genreId, CancellationToken cancellationToken = default);
}
