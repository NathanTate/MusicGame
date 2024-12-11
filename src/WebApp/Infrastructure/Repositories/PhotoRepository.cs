//using Application.InfrastructureInterfaces;
//using Domain.Entities;
//using Domain.Enums;
//using Infrastructure.Context;
//using Microsoft.EntityFrameworkCore;

//namespace Infrastructure.Repositories;
//internal class PhotoRepository : IPhotoRepository
//{
//    private readonly AppDbContext _dbContext;
//    public PhotoRepository(AppDbContext dbContext)
//    {
//        _dbContext = dbContext;
//    }

//    public async Task<Photo?> GetByIdAsync(int photoId, bool tracking, CancellationToken cancellationToken = default)
//    {
//        var query = _dbContext.Photos;

//        if (tracking)
//        {
//            query.AsNoTracking();
//        }

//        return await query.FirstOrDefaultAsync(x => x.PhotoId == photoId);
//    }

//    public async Task<string?> DeleteAsync(int photoId, CancellationToken cancellationToken = default)
//    {
//        var photo = await _dbContext.Photos.FindAsync(photoId, cancellationToken);

//        if (photo is null)
//        {
//            return null;
//        }

//        _dbContext.Entry(photo).State = EntityState.Deleted;
//        return photo.URL;
//    }

//}
