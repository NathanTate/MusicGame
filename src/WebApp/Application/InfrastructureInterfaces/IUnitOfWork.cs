using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.InfrastructureInterfaces;
public interface IUnitOfWork
{
    IGenreRepository GenreRepository { get; }
    ISongRepository SongRepository { get; }
    IPhotoRepository PhotoRepository { get; }

    Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TEntity : class;
    bool Exists<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
