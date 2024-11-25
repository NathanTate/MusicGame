using Application.Authorization;
using Application.DTO.Playlists;
using Application.Errors;
using Application.InfrastructureInterfaces;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Services;
internal class PlaylistService : IPlaylistService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileHandler _fileHandler;
    private readonly IPlaylistAuthorizationService _playlistAuthorizationService;    
    public PlaylistService(IUnitOfWork uow, IMapper mapper, IFileHandler fileHandler, IPlaylistAuthorizationService playlistAuthorizationService)
    {
        _uow = uow;
        _mapper = mapper;
        _fileHandler = fileHandler;
        _playlistAuthorizationService = playlistAuthorizationService;
    }

    public async Task<PlaylistResponse> CreatePlaylistAsync(string userId, CancellationToken cancellationToken = default)
    {
        int playlistCount = await _uow.CountAsync<Playlist>(cancellationToken);

        Playlist playlist = new Playlist()
        {
            Name = $"My Playlist {playlistCount + 1}",
            IsPrivate = false,
            TotalDuration = 0,
            UserId = userId,
        };

        _uow.PlaylistRepository.Create(playlist);
        await _uow.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PlaylistResponse>(playlist);
    }

    public async Task<Result<PlaylistResponse>> GetPlaylistAsync(int playlistId, string? userId, CancellationToken cancellationToken = default)
    {
        var playlist = await _uow.PlaylistRepository.GetByIdAsync(playlistId, false, userId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        return Result.Ok(_mapper.Map<PlaylistResponse>(playlist));
    }

    public async Task<List<PlaylistResponse>> GetPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        var playlists = await _uow.PlaylistRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<List<PlaylistResponse>>(playlists);
    }

    public async Task<Result<PlaylistResponse>> UpdatePlaylistAsync(UpdatePlaylistRequest model, CancellationToken cancellationToken = default)
    {
        var playlist = await _uow.PlaylistRepository.GetByIdAsync(model.PlaylistId, cancellationToken: cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {model.PlaylistId} cannot be found");
        }

        if (!_playlistAuthorizationService.Authorize(playlist, OperationType.Update))
        {
            return new ForbiddenAccessError();
        }

        _mapper.Map(model, playlist);

        _uow.PlaylistRepository.Update(playlist);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok(_mapper.Map<PlaylistResponse>(playlist));
    }
    public async Task<Result> DeletePlaylistAsync(int playlistId, string userId, CancellationToken cancellationToken = default)
    {
        var playlist = await _uow.PlaylistRepository.GetByIdAsync(playlistId, false, userId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        if (!_playlistAuthorizationService.Authorize(playlist, OperationType.Patch))
        {
            return new ForbiddenAccessError();
        }

        _uow.PlaylistRepository.Delete(playlist);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<PlaylistResponse>> UploadPhotoAsync(int playlistId, IFormFile photo, CancellationToken cancellationToken = default)
    {
        var playlist = await _uow.PlaylistRepository.GetByIdAsync(playlistId, cancellationToken: cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        if (!_playlistAuthorizationService.Authorize(playlist, OperationType.Patch))
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
        await _uow.SaveChangesAsync();

        return Result.Ok(_mapper.Map<PlaylistResponse>(playlist));
    }
    public async Task<Result> DeletePhotoAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var playlist = await _uow.PlaylistRepository.GetByIdAsync(playlistId, cancellationToken: cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {playlistId} cannot be found");
        }

        if (!_playlistAuthorizationService.Authorize(playlist, OperationType.Delete))
        {
            return new ForbiddenAccessError();
        }

        if (playlist.Photo is null)
        {
            return Result.Ok();
        }

        var fileName = Path.GetFileName(playlist.Photo.URL);
        await _fileHandler.DeleteFileAsync(fileName, FileContainer.Photos, cancellationToken);

        await _uow.PhotoRepository.DeleteAsync(playlist.Photo.PhotoId);
        await _uow.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> AddSongToPlaylistAsync(UpsertSongPlaylistRequest model, string userId, CancellationToken cancellationToken = default)
    {
        var playlist = await _uow.PlaylistRepository.GetByIdAsync(model.PlaylistId, false, userId, cancellationToken);

        if (playlist is null)
        {
            return new NotFoundError($"Playlist with id {model.PlaylistId} cannot be found");
        }

        if (!_playlistAuthorizationService.Authorize(playlist, OperationType.Create))
        {
            return new ForbiddenAccessError();
        }

        var songExists = await _uow.ExistsAsync<Song>(x => x.SongId == model.SongId, cancellationToken);

        if (!songExists)
        {
            return new NotFoundError($"Song with id {model.SongId} cannot be found");
        }

        var playlistSong = await _uow.PlaylistRepository.GetPlaylistSongAsync(model.SongId, model.PlaylistId, cancellationToken: cancellationToken);

        if (playlistSong is not null)
        {
            return new ValidationError($"Song with id {model.SongId} is already in the playlist");
        }

        _uow.PlaylistRepository.AddSongToPlaylist(model.SongId, model.PlaylistId, model.Position);
        await _uow.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> RemoveSongFromPlaylistAsync(int songId, int playlistId, CancellationToken cancellationToken = default)
    {
        var playlistSong = await _uow.PlaylistRepository.GetPlaylistSongAsync(songId, playlistId, cancellationToken: cancellationToken);

        if (playlistSong is null)
        {
            return new NotFoundError($"Song with id {songId} does not exist in Playlist with id {playlistId}");
        }

        if (!_playlistAuthorizationService.Authorize(playlistSong.Playlist, OperationType.Delete))
        {
            return new ForbiddenAccessError();
        }

        _uow.PlaylistRepository.RemoveSongFromPlaylist(playlistSong);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> UpdateSongPositionAsync(UpsertSongPlaylistRequest model, CancellationToken cancellationToken = default)
    {
        var playlistSong = await _uow.PlaylistRepository.GetPlaylistSongAsync(model.SongId, model.PlaylistId, cancellationToken: cancellationToken);

        if (playlistSong is null)
        {
            return new NotFoundError($"Song with id {model.SongId} does not exist in Playlist with id {model.PlaylistId}");
        }

        if (!_playlistAuthorizationService.Authorize(playlistSong.Playlist, OperationType.Delete))
        {
            return new ForbiddenAccessError();
        }

        _mapper.Map(model, playlistSong);

        _uow.PlaylistRepository.UpdateSongPosition(playlistSong);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<bool> IsPlaylistNameAvailable(string name, CancellationToken cancellationToken = default)
    {
        return await _uow.ExistsAsync<Playlist>(p => p.Name.ToUpper() == name.ToUpper(), cancellationToken);
    }
}
