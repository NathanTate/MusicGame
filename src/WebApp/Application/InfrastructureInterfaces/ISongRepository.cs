using Domain.Entities;

namespace Application.InfrastructureInterfaces;
public interface ISongRepository
{
    Task<List<Song>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Song?> GetByIdAsync(int songId, CancellationToken cancellationToken = default);
    Song Create(Song model);
    Song Update(Song model);
    Task<string?> DeleteAsync(int songId, CancellationToken cancellationToken = default);
}
