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
using Application.Services.Elastic;
using GeniusLyrics.NET;
using Application.Models.Elastic;

namespace Application.Services;

internal class SongService : ISongService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IFileHandler _fileHandler;
    private readonly IUserContext _userContext;
    private readonly SongsElasticService _elasticService;
    private readonly GeniusClient _geniusClient;
    public SongService(AppDbContext dbContext, SongsElasticService elasticService, IMapper mapper,
        IFileHandler fileHandler, IUserContext userContext, GeniusClient geniusClient)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _fileHandler = fileHandler;
        _userContext = userContext;
        _elasticService = elasticService;
        _geniusClient = geniusClient;
    }

    public async Task<Result<bool>> ToggleLikeAsync(int songId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        bool liked = false;
        var like = await _dbContext.SongLike.FindAsync([songId, currentUser.UserId], cancellationToken);

        if (like is not null)
        {
            _dbContext.Remove(like);
        }
        else
        {
            var playlist = await _dbContext.Songs.AsNoTracking().FirstOrDefaultAsync(x => x.SongId == songId);

            if (playlist is null)
            {
                return new ValidationError($"Can't add like to not existed song - {songId}");
            }
            if (playlist.UserId == currentUser.UserId)
            {
                return new ValidationError("You can't like your own songs");
            }

            var songLike = new SongLike
            {
                SongId = songId,
                UserId = currentUser.UserId
            };

            _dbContext.SongLike.Add(songLike);
            liked = true;
        }

        await _dbContext.SaveChangesAsync();
        return Result.Ok(liked);
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
        await _elasticService.AddOrUpdateAsync(_mapper.Map<SongDoc>(song), ElasticIndex.SongsIndex, cancellationToken);

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

        var songWithLyrics = await _geniusClient.GetSong(song.Name, song.ArtistName);
        song.Lyrics = songWithLyrics?.Lyrics;

        var likesCount = await _dbContext.SongLike.Where(x => x.SongId == song.SongId).CountAsync(cancellationToken);
        song.LikesCount = likesCount;


        return Result.Ok(song);
    }

    public async Task<PagedList<SongResponse>> GetSongsAsync(SongsQuery query, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUser()?.UserId;

        var songsQuery = _dbContext.Songs
            .AsNoTracking()
            .Where(x => !x.IsPrivate || x.UserId == userId);

        if (query.SortOrder == "desc")
        {
            songsQuery = songsQuery.OrderByDescending(query.GetSortProperty());
        }
        else
        {
            songsQuery = songsQuery.OrderBy(query.GetSortProperty());
        }

        List<int> searchIds = new();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var elasticQuery = new ElasticQuery
            {
                PageSize = query.PageSize,
                Page = query.Page,
                SearchTerm = query.SearchTerm
            };

            var result = await _elasticService.SearchAsync(elasticQuery, ElasticIndex.SongsIndex, cancellationToken);

            if (result.IsFailed)
            {
                throw new ArgumentNullException(result.Errors.Select(x => x.Message).FirstOrDefault());
            }

            searchIds = result.Value.Select(x => Convert.ToInt32(x.Id)).ToList();

            songsQuery = songsQuery.Where(x => searchIds.Contains(x.SongId));
        }

        var pagedList = await PagedList<SongResponse>.CreateAsync(songsQuery.ProjectTo<SongResponse>(_mapper.ConfigurationProvider), query.Page, query.PageSize);

        var songIds = pagedList.Items.Select(x => x.SongId);

        var songsLikes = _dbContext.SongLike
            .AsNoTracking()
            .Where(x => songIds.Contains(x.SongId))
            .GroupBy(x => x.SongId)
            .Select(x => new
            {
                SongId = x.Key,
                LikesCount = x.Count(),
            });

        foreach (var item in songsLikes)
        {
            var matchingItem = pagedList.Items.Find(x => x.SongId == item.SongId);
            if (matchingItem is not null)
            {
                matchingItem.LikesCount = item.LikesCount;
            }
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var orderedItems = pagedList.Items.OrderBy(x => searchIds.IndexOf(x.SongId)).ToList();

            return new PagedList<SongResponse>(orderedItems, pagedList.Page, pagedList.PageSize, pagedList.TotalCount);
        }

        return pagedList;
    }

    public async Task<PagedList<SongResponse>> GetByIdsAsync(IEnumerable<int> ids, BaseQuery query, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUser()?.UserId;

        var songsQuery = _dbContext.Songs
            .AsNoTracking()
            .Where(x => !x.IsPrivate || x.UserId == userId)
            .Where(x => ids.Contains(x.SongId));

        var pagedList = await PagedList<SongResponse>.CreateAsync(songsQuery.ProjectTo<SongResponse>(_mapper.ConfigurationProvider), query.Page, query.PageSize);

        var songsLikes = _dbContext.SongLike
            .AsNoTracking()
            .Where(x => ids.Contains(x.SongId))
            .GroupBy(x => x.SongId)
            .Select(x => new
            {
                SongId = x.Key,
                LikesCount = x.Count(),
            });

        foreach (var item in songsLikes)
        {
            var matchingItem = pagedList.Items.Find(x => x.SongId == item.SongId);
            if (matchingItem is not null)
            {
                matchingItem.LikesCount = item.LikesCount;
            }
        }

        return pagedList;
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
        await _elasticService.AddOrUpdateAsync(_mapper.Map<SongDoc>(song), ElasticIndex.SongsIndex, cancellationToken);

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
        await _elasticService.Remove(song.SongId.ToString(), ElasticIndex.SongsIndex, cancellationToken);

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

        var photo = await _dbContext.Photos.FindAsync([song.Photo.PhotoId], cancellationToken);

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

    public async Task<Result<bool>> IsLiked(int songId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }


        var like = await _dbContext.SongLike.AnyAsync(x => x.SongId == songId && x.UserId == currentUser.UserId, cancellationToken);

        return Result.Ok(like);
    }

    private async Task<Song?> GetByIdAsync(int songId, string? userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Songs
            .Include(x => x.Photo)
            .Where(x => x.SongId == songId && (!x.IsPrivate || x.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void AttachGenresToSong(Song song, List<int> genreIds)
    {
        song.Genres.RemoveAll(x => !genreIds.Contains(x.GenreId));

        foreach (var id in genreIds)
        {
            var genre = song.Genres.Find(x => x.GenreId == id);
            var genreName = _dbContext.Genres.AsNoTracking().FirstOrDefault(x => x.GenreId == id)?.Name ?? "";
            if (genre is null)
            {
                genre = new Genre() { GenreId = id, Name = genreName };
                _dbContext.Attach(genre);
                song.Genres.Add(genre);
            }
        }
    }
}
