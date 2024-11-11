using Domain.Entities;

namespace Application.InfrastructureInterfaces;
public interface IPhotoRepository
{
    Task<Photo?> GetByIdAsync(int photoId, bool tracking, CancellationToken cancellationToken = default);
    Task<string?> DeleteAsync(int photoId, CancellationToken cancellationToken = default);
}
