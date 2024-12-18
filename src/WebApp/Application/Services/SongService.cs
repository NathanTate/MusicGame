using Application.Models.Songs;
using Application.Errors;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using Application.Common.UserHelpers;
using Application.Models.Queries;
using Application.Models;
using System.Linq.Expressions;

namespace Application.Services;

internal class SongService : ISongService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IFileHandler _fileHandler;
    private readonly IUserContext _userContext;
    public SongService(AppDbContext dbContext, IMapper mapper, IFileHandler fileHandler, IUserContext userContext)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _fileHandler = fileHandler;
        _userContext = userContext;
    }

    public async Task<Result<SongResponse>> CreateSongAsync(CreateSongRequest model, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var exists = await _dbContext.Songs.AnyAsync(s => s.Name.ToUpper() == model.Name.ToUpper(), cancellationToken);

        if (exists)
        {
            return new ValidationError($"Song with name {model.Name} already exists");
        }

        var allExist = model.GenreIds.TrueForAll(id => _dbContext.Genres.Any(e => e.GenreId == id));

        if (!allExist)
        {
            return new NotFoundError($"One or many of provided genres do not exists");
        }

        var song = _mapper.Map<Song>(model);
        AttachGenresToSong(song, model.GenreIds);

        var songUrl = await _fileHandler.UploadFileAsync(model.SongFile, FileContainer.Songs, cancellationToken);
        song.Url = songUrl;
        song.UserId = currentUser.UserId;

        _dbContext.Songs.Add(song);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(_mapper.Map<SongResponse>(song));
    }

    public async Task<Result<SongResponse>> GetSongAsync(int songId, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUser()?.UserId;

        var song = await _dbContext.Songs
            .AsNoTracking()
            .Where(x => x.SongId == songId && (!x.IsPrivate || x.UserId == userId))
            .ProjectTo<SongResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {songId} cannot be found");
        }

        return Result.Ok(song);
    }

    public async Task<PagedList<SongResponse>> GetSongsAsync(SongsQueryRequest query, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUser()?.UserId;

        var songsQuery = _dbContext.Songs
            .AsNoTracking()
            .Where(x => (!x.IsPrivate || x.UserId == userId));

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            songsQuery = songsQuery.Where(x =>
                x.Name.ToLower().Contains(query.SearchTerm.ToLower()) ||
                x.User.DisplayName.ToLower().Contains(query.SearchTerm.ToLower()) ||
                x.Genres.Any(x => x.NormalizedName.Contains(query.SearchTerm.ToUpper())));
        }

        if (query.SortOrder == "desc")
        {
            songsQuery = songsQuery.OrderByDescending(GetSortProperty(query));
        }
        else
        {
            songsQuery = songsQuery.OrderBy(GetSortProperty(query));
        }

        return await PagedList<SongResponse>.CreateAsync(songsQuery.ProjectTo<SongResponse>(_mapper.ConfigurationProvider), query.Page, query.PageSize);
    }

    public async Task<Result<SongResponse>> UpdateSongAsync(UpdateSongRequest model, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var song = await _dbContext.Songs
            .Include(x => x.Genres)
            .Where(x => x.SongId == model.SongId && (!x.IsPrivate || x.UserId == currentUser.UserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {model.SongId} cannot be found");
        }

        if (song.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        var nameExists = await _dbContext.Songs.AnyAsync(x => x.Name.ToUpper() == model.Name.ToUpper() && x.SongId != model.SongId, cancellationToken);

        if (nameExists)
        {
            return new NotFoundError($"Song with name {model.Name} already exists");
        }

        var allExist = model.GenreIds.TrueForAll(id => _dbContext.Genres.Any(e => e.GenreId == id));

        if (!allExist)
        {
            return new NotFoundError($"One or many of provided genres do not exists");
        }

        _mapper.Map(model, song);

        AttachGenresToSong(song, model.GenreIds);

        await _dbContext.SaveChangesAsync();

        return Result.Ok(_mapper.Map<SongResponse>(song));
    }

    public async Task<Result> DeleteSongAsync(int songId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var song = await GetByIdAsync(songId, currentUser.UserId, cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {songId} cannot be found");
        }

        if (song.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        _dbContext.Songs.Remove(song);
        var fileName = Path.GetFileName(song.Url);

        await _fileHandler.DeleteFileAsync(fileName, FileContainer.Songs, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<SongResponse>> UploadPhotoAsync(int songId, IFormFile photo, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var song = await GetByIdAsync(songId, currentUser.UserId, cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {songId} cannot be found");
        }

        if (song.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        var photoUrl = song.Photo is null
            ? await _fileHandler.UploadFileAsync(photo, FileContainer.Photos, cancellationToken)
            : await _fileHandler.UpdateFileAsync(Path.GetFileName(song.Photo.URL), photo, FileContainer.Photos, cancellationToken);

        var songPhoto = new Photo()
        {
            URL = photoUrl,
            Size = photo.Length,
            ContentType = photo.ContentType,
        };

        song.Photo = songPhoto;

        await _dbContext.SaveChangesAsync();

        return Result.Ok(_mapper.Map<SongResponse>(song));
    }

    public async Task<Result> DeletePhotoAsync(int songId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var song = await GetByIdAsync(songId, currentUser.UserId, cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {songId} cannot be found");
        }

        if (song.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        if (song.Photo is null)
        {
            return Result.Ok();
        }

        var fileName = Path.GetFileName(song.Photo.URL);
        await _fileHandler.DeleteFileAsync(fileName, FileContainer.Photos, cancellationToken);

        var photo = await _dbContext.Photos.FindAsync(song.Photo.PhotoId, cancellationToken);

        if (photo is null)
        {
            return new NotFoundError($"Photo for playlist {song.Name} cannot be found");
        }

        _dbContext.Remove(photo);

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<bool> IsSongNameAvailableAsync(string name, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Songs.AnyAsync(p => p.Name.ToUpper() == name.ToUpper(), cancellationToken);
        return !exists;
    }

    private async Task<Song?> GetByIdAsync(int songId, string? userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Songs
            .Where(x => x.SongId == songId && (!x.IsPrivate || x.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void AttachGenresToSong(Song song, List<int> genreIds)
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

    private static Expression<Func<Song, object>> GetSortProperty(SongsQueryRequest query) =>
        query.SortColumn?.ToLower() switch
        {
            "name" => song => song.Name,
            "likes" => song => song.LikesCount,
            "duration" => song => song.Duration,
            "releaseDate" => song => song.ReleaseDate,
            "createdDate" => song => song.CreatedAt,
            _ => song => song.SongId
        };
}
