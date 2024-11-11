using Application.InfrastructureInterfaces;
using Domain.Entities;
using Infrastructure.Context;

namespace Infrastructure.Repositories;
internal class PlaylistRepository : IPlaylistRepository
{
    private readonly AppDbContext _dbContext;
    public PlaylistRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(Playlist playlist)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Playlist>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Playlist?> GetByIdAsync(int playlistId, bool tracking, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Update(Playlist playlist)
    {
        throw new NotImplementedException();
    }
}
