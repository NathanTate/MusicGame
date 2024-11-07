using Domain.Entities;

namespace Application.InfrastructureInterfaces;
public interface IGenreRepository
{
    Task<List<Genre>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Genre?> GetByIdAsync(int genreId, CancellationToken cancellationToken = default);
    void Create(Genre model);
    void Update(Genre model);
    Task<bool> DeleteAsync(int genreId, CancellationToken cancellationToken = default);
}
