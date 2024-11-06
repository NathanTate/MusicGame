using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.InfrastructureInterfaces;
public interface IUnitOfWork
{
    IGenreRepository GenreRepository { get; }

    Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
