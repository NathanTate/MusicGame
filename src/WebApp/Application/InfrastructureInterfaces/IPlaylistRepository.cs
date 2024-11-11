using Domain.Entities;

namespace Application.InfrastructureInterfaces;
public interface IPlaylistRepository
{
    Task<List<Playlist>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Playlist?> GetByIdAsync(int playlistId, bool tracking, CancellationToken cancellationToken = default);
    void Create(Playlist playlist);
    void Update(Playlist playlist);
    Task<bool> DeleteAsync(int playlistId, CancellationToken cancellationToken = default);
}
