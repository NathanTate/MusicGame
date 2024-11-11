using Application.InfrastructureInterfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;
internal class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private IGenreRepository? _genreRepository;
    private ISongRepository? _songRepository;
    private IPhotoRepository? _photoRepository;
    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IGenreRepository GenreRepository { get => _genreRepository ??= new GenreRepository(_dbContext); }
    public ISongRepository SongRepository { get => _songRepository ??= new SongRepository(_dbContext); }
    public IPhotoRepository PhotoRepository { get => _photoRepository ??= new PhotoRepository(_dbContext); }

    public async Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TEntity : class
    {
        return await _dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public bool Exists<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        return _dbContext.Set<TEntity>().Any(predicate);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
