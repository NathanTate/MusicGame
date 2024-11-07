using Application.InfrastructureInterfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
internal class GenreRepository : IGenreRepository
{
    private readonly AppDbContext _dbContext;
    public GenreRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(Genre model)
    {
        _dbContext.Genres.Add(model);
    }

    public async Task<bool> DeleteAsync(int genreId, CancellationToken cancellationToken = default)
    {
        var genre = await _dbContext.Genres.FindAsync([genreId], cancellationToken: cancellationToken);

        if (genre is null)
        {
            return false;
        }

        _dbContext.Entry(genre).State = EntityState.Deleted;
        return true;
    }

    public async Task<List<Genre>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Genres.ToListAsync(cancellationToken);
    }

    public async Task<Genre?> GetByIdAsync(int genreId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Genres.FindAsync([genreId], cancellationToken: cancellationToken);
    }

    public void Update(Genre model)
    {
        _dbContext.Update(model);
    }
}
