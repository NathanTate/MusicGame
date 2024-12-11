using Application.Models.Playlists;
using Application.Errors;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Infrastructure.Context;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Application.Common.UserHelpers;
using AutoMapper.QueryableExtensions;
using Application.Models;
using Application.Models.Queries;
using System.Linq.Expressions;
using Application.Models.Users;

namespace Application.Services;
internal class PlaylistService : IPlaylistService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IFileHandler _fileHandler;
    private readonly IUserContext _userContext;
    public PlaylistService(AppDbContext dbContext, IMapper mapper, IFileHandler fileHandler, IUserContext userContext)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _fileHandler = fileHandler;
        _userContext = userContext;
    }

    public async Task<Result<PlaylistResponse>> CreatePlaylistAsync(CancellationToken cancellationToken = default)
    {
        int playlistCount = await _dbContext.Playlists.CountAsync(cancellationToken);
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        Playlist playlist = new Playlist()
        {
            Name = $"My Playlist {playlistCount + 1}",
            IsPrivate = false,
            TotalDuration = 0,
            UserId = currentUser.UserId,
        };

        _dbContext.Playlists.Add(playlist);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PlaylistResponse>(playlist);
    }

    public async Task<Result<PlaylistResponse>> GetPlaylistAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUser()?.UserId;

        var playlist = await _dbContext.Playlists
            .AsNoTracking()
            .Where(x => x.PlaylistId == playlistId && (!x.IsPrivate || x.UserId == userId))
            .ProjectTo<PlaylistResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        return Result.Ok(playlist);
    }

    public async Task<PagedList<PlaylistResponse>> GetPlaylistsAsync(PlaylistsQueryRequest query, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUser()?.UserId;

        var playlistsQuery = _dbContext.Playlists
            .AsNoTracking()
            .Where(x => (!x.IsPrivate || x.UserId == userId));

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            playlistsQuery = playlistsQuery.Where(x =>
                x.Name.ToLower().Contains(query.SearchTerm.ToLower()) ||
                x.Description != null && x.Description.ToLower().Contains(query.SearchTerm.ToLower()));
        }

        if (query.SortOrder == "desc")
        {
            playlistsQuery = playlistsQuery.OrderByDescending(GetSortProperty(query));
        }
        else
        {
            playlistsQuery = playlistsQuery.OrderBy(GetSortProperty(query));
        }

        var resultQuery = playlistsQuery.Select(x => new PlaylistResponse
        {
            PlaylistId = x.PlaylistId,
            Name = x.Name,
            Description = x.Description,
            IsPrivate = x.IsPrivate,
            TotalDuration = x.TotalDuration,
            SongsCount = x.SongsCount,
            LikesCount = x.LikesCount,
            CreatedAt = x.CreatedAt,
            PhotoUrl = x.Photo == null ? null : x.Photo.URL,
            User = new ArtistResponse(x.User.Id, x.User.Email, x.User.DisplayName)
        });

        return await PagedList<PlaylistResponse>.CreateAsync(resultQuery, query.Page, query.PageSize, cancellationToken); ;
    }

    public async Task<Result<PlaylistResponse>> UpdatePlaylistAsync(UpdatePlaylistRequest model, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlist = await GetByIdAsync(model.PlaylistId, currentUser.UserId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {model.PlaylistId} cannot be found");
        }

        if (playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        _mapper.Map(model, playlist);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(_mapper.Map<PlaylistResponse>(playlist));
    }
    public async Task<Result> DeletePlaylistAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlist = await GetByIdAsync(playlistId, currentUser.UserId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        if (playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        _dbContext.Playlists.Remove(playlist);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<PlaylistResponse>> UploadPhotoAsync(int playlistId, IFormFile photo, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlist = await GetByIdAsync(playlistId, currentUser.UserId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        if (playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        var photoUrl = playlist.Photo is null
            ? await _fileHandler.UploadFileAsync(photo, FileContainer.Photos, cancellationToken)
            : await _fileHandler.UpdateFileAsync(Path.GetFileName(playlist.Photo.URL), photo, FileContainer.Photos, cancellationToken);

        var playlistPhoto = new Photo()
        {
            URL = photoUrl,
            Size = photo.Length,
            ContentType = photo.ContentType,
        };

        playlist.Photo = playlistPhoto;
        await _dbContext.SaveChangesAsync();

        return Result.Ok(_mapper.Map<PlaylistResponse>(playlist));
    }
    public async Task<Result> DeletePhotoAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlist = await GetByIdAsync(playlistId, currentUser.UserId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        if (playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        if (playlist.Photo is null)
        {
            return Result.Ok();
        }

        var fileName = Path.GetFileName(playlist.Photo.URL);
        await _fileHandler.DeleteFileAsync(fileName, FileContainer.Photos, cancellationToken);

        var photo = await _dbContext.Photos.FindAsync(playlist.Photo.PhotoId, cancellationToken);

        if (photo is null)
        {
            return new NotFoundError($"Photo for playlist {playlist.Name} cannot be found");
        }

        _dbContext.Remove(photo);

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> AddSongToPlaylistAsync(UpsertSongPlaylistRequest model, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlist = await GetByIdAsync(model.PlaylistId, currentUser.UserId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {model.PlaylistId} cannot be found");
        }

        if (playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        var songExists = await _dbContext.Songs.AnyAsync(x => x.SongId == model.SongId, cancellationToken);

        if (!songExists)
        {
            return new NotFoundError($"Song with id {model.SongId} cannot be found");
        }

        var playlistSong = await _dbContext.PlaylistSongs
            .Include(p => p.Playlist)
            .FirstOrDefaultAsync(x => x.SongId == model.SongId && x.PlaylistId == model.PlaylistId, cancellationToken);

        if (playlistSong is not null)
        {
            return new ValidationError($"Song with id {model.SongId} is already in the playlist");
        }

        playlistSong = new PlaylistSong
        {
            PlaylistId = model.PlaylistId,
            SongId = model.SongId,
            Position = model.Position,
        };

        _dbContext.PlaylistSongs.Add(playlistSong);

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> RemoveSongFromPlaylistAsync(int songId, int playlistId, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlistSong = await _dbContext.PlaylistSongs
            .Include(p => p.Playlist)
            .FirstOrDefaultAsync(x => x.SongId == songId && x.PlaylistId == playlistId, cancellationToken);

        if (playlistSong is null)
        {
            return new NotFoundError($"Song with id {songId} does not exist in Playlist with id {playlistId}");
        }

        if (playlistSong.Playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        _dbContext.PlaylistSongs.Remove(playlistSong);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> UpdateSongPositionAsync(UpsertSongPlaylistRequest model, CancellationToken cancellationToken = default)
    {
        var currentUser = _userContext.GetCurrentUser();

        if (currentUser is null)
        {
            return new ValidationError("User doesn't exist");
        }

        var playlistSong = await _dbContext.PlaylistSongs
            .Include(p => p.Playlist)
            .FirstOrDefaultAsync(x => x.SongId == model.SongId && x.PlaylistId == model.PlaylistId, cancellationToken);

        if (playlistSong is null)
        {
            return new NotFoundError($"Song with id {model.SongId} does not exist in Playlist with id {model.PlaylistId}");
        }

        if (playlistSong.Playlist.UserId != currentUser.UserId)
        {
            return new ForbiddenAccessError();
        }

        _mapper.Map(model, playlistSong);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<bool> IsPlaylistNameAvailable(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Playlists.AnyAsync(p => p.Name.ToUpper() == name.ToUpper(), cancellationToken);
    }

    private async Task<Playlist?> GetByIdAsync(int playlistId, string? userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Playlists
            .Where(x => x.PlaylistId == playlistId && (!x.IsPrivate || x.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Expression<Func<Playlist, object>> GetSortProperty(PlaylistsQueryRequest query) =>
        query.SortColumn.ToLower() switch
        {
            "name" => playlist => playlist.Name,
            "likes" => playlist => playlist.LikesCount,
            "duration" => playlist => playlist.TotalDuration,
            "songsCount" => playlist => playlist.SongsCount,
            _ => playlist => playlist.PlaylistId
        };

}
