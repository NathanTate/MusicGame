using Application.Models;
using Application.Models.Genres;
using Application.Models.Queries;
using FluentResults;

namespace Application.Interfaces;
public interface IGenreService
{
    Task<PagedList<GenreResponse>> GetGenresAsync(GenresQueryRequest query, CancellationToken cancellationToken = default);
    Task<Result<GenreResponse>> GetGenreAsync(int genreId, CancellationToken cancellationToken = default);
    Task<Result<GenreResponse>> CreateGenreAsync(CreateGenreRequest model, CancellationToken cancellationToken = default);
    Task<Result<GenreResponse>> UpdateGenreAsync(UpdateGenreRequest model, CancellationToken cancellationToken = default);
    Task<Result> DeleteGenreAsync(int genreId, CancellationToken cancellationToken = default);
    Task<bool> IsGenreNameAvailable(string name, CancellationToken cancellationToken = default);
}
