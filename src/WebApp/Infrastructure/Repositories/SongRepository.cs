using Application.InfrastructureInterfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
internal class SongRepository : ISongRepository
{
    private readonly AppDbContext _dbContext;
    public SongRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Song Create(Song model)
    {
        _dbContext.Songs.Add(model);
        return model;
    }

    public async Task<string?> DeleteAsync(int songId, CancellationToken cancellationToken = default)
    {
        var song = await _dbContext.Songs.FindAsync([songId], cancellationToken: cancellationToken);

        if (song is null)
        {
            return null;
        }

        _dbContext.Entry(song).State = EntityState.Deleted;
        return song.Url;
    }

    public async Task<List<Song>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Songs.ToListAsync(cancellationToken);
    }

    public async Task<Song?> GetByIdAsync(int songId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Songs.FindAsync([songId], cancellationToken: cancellationToken);
    }

    public Song Update(Song model)
    {
        _dbContext.Songs.Update(model);
        return model;
    }
}
