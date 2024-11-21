using Domain.Entities;

namespace Application.InfrastructureInterfaces;
public interface ISongRepository
{
    Task<List<Song>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Song?> GetByIdAsync(int songId, bool tracking = true, string? userId = null, CancellationToken cancellationToken = default);
    void Create(Song model);
    void Update(Song model);
    Task<bool> DeleteAsync(int songId, CancellationToken cancellationToken = default);
    void AttachGenresToSong(Song song, List<int> genreIds);
}
