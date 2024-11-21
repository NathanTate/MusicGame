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

    public void AttachGenresToSong(Song song, List<int> genreIds)
    {
        song.Genres.RemoveAll(x => !genreIds.Contains(x.GenreId));

        foreach (var id in genreIds)
        {
            var genre = song.Genres.Find(x => x.GenreId == id);
            if (genre is null)
            {
                genre = new Genre() { GenreId = id };
                _dbContext.Attach(genre);
                song.Genres.Add(genre);
            }
        }
    }

    public void Create(Song model)
    {
        _dbContext.Songs.Add(model);
    }

    public async Task<bool> DeleteAsync(int songId, CancellationToken cancellationToken = default)
    {
        var song = await _dbContext.Songs.FindAsync([songId], cancellationToken: cancellationToken);

        if (song is null)
        {
            return false;
        }

        _dbContext.Entry(song).State = EntityState.Deleted;
        return true;
    }

    public async Task<List<Song>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Songs
            .AsNoTracking()
            .Include(x => x.Photo)
            .ToListAsync(cancellationToken);
    }

    public async Task<Song?> GetByIdAsync(int songId, bool tracking = true, string? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Songs
            .Where(x => x.SongId == songId && (!x.IsPrivate || x.UserId == userId))
            .Include(x => x.Photo)
            .Include(x => x.Genres)
            .AsSplitQuery();

        if (!tracking)
        {
            query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);       
    }

    public void Update(Song model)
    {
        _dbContext.Songs.Update(model);
    }
}
