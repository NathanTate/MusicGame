using Domain.Entities;

namespace Application.InfrastructureInterfaces;
public interface IGenreRepository
{
    Task<List<Genre>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Genre?> GetByIdAsync(int genreId, CancellationToken cancellationToken = default);
    Genre Create(Genre model);
    Genre Update(Genre model);
    Task<bool> DeleteAsync(int genreId, CancellationToken cancellationToken = default);
}
