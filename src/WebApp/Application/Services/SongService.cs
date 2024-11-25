using Application.Authorization;
using Application.DTO.Songs;
using Application.Errors;
using Application.InfrastructureInterfaces;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

internal class SongService : ISongService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileHandler _fileHandler;
    private readonly ISongAuthorizationService _songAuthorizationService;
    public SongService(IUnitOfWork uow, IMapper mapper, IFileHandler fileHandler, ISongAuthorizationService songAuthorizationService)
    {
        _uow = uow;
        _mapper = mapper;
        _fileHandler = fileHandler;
        _songAuthorizationService = songAuthorizationService;
    }

    public async Task<Result<SongResponse>> CreateSongAsync(CreateSongRequest model, string userId, CancellationToken cancellationToken = default)
    {
        var exists = await _uow.ExistsAsync<Song>(s => s.Name.ToUpper() == model.Name.ToUpper(), cancellationToken);

        if (exists)
        {
            return new ValidationError($"Song with name {model.Name} already exists");
        }

        var allExist = model.GenreIds.TrueForAll(id => _uow.Exists<Genre>(e => e.GenreId == id));

        if (!allExist)
        {
            return new NotFoundError($"One or many of provided genres do not exists");
        }

        var song = _mapper.Map<Song>(model);
        _uow.SongRepository.AttachGenresToSong(song, model.GenreIds);

        var songUrl = await _fileHandler.UploadFileAsync(model.SongFile, FileContainer.Songs, cancellationToken);
        song.Url = songUrl;
        song.UserId = userId;

        _uow.SongRepository.Create(song);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok(_mapper.Map<SongResponse>(song));
    }

    public async Task<Result<SongResponse>> GetSongAsync(int songId, string? userId, CancellationToken cancellationToken = default)
    {
        var song = await _uow.SongRepository.GetByIdAsync(songId, false, userId, cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {songId} cannot be found");
        }

        return Result.Ok(_mapper.Map<SongResponse>(song));
    }

    public async Task<List<SongResponse>> GetSongsAsync(CancellationToken cancellationToken = default)
    {
        var songs = await _uow.SongRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<List<SongResponse>>(songs);  
    }

    public async Task<Result<SongResponse>> UpdateSongAsync(UpdateSongRequest model, CancellationToken cancellationToken = default)
    {
        var song = await _uow.SongRepository.GetByIdAsync(model.SongId, cancellationToken: cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {model.SongId} cannot be found");
        }

        if (!_songAuthorizationService.Authorize(song, OperationType.Update))
        {
            return new ForbiddenAccessError();
        }

        var nameExists = await _uow.ExistsAsync<Song>(x => x.Name.ToUpper() == model.Name.ToUpper() && x.SongId != model.SongId, cancellationToken);

        if (nameExists)
        {
            return new NotFoundError($"Song with name {model.Name} already exists");
        }

        var allExist = model.GenreIds.TrueForAll(id => _uow.Exists<Genre>(e => e.GenreId == id));

        if (!allExist)
        {
            return new NotFoundError($"One or many of provided genres do not exists");
        }

        _mapper.Map(model, song);

        _uow.SongRepository.AttachGenresToSong(song, model.GenreIds);
        _uow.SongRepository.Update(song);

        await _uow.SaveChangesAsync();

        return Result.Ok(_mapper.Map<SongResponse>(song));
    }

    public async Task<Result> DeleteSongAsync(int songId, CancellationToken cancellationToken = default)
    {
        var song = await _uow.SongRepository.GetByIdAsync(songId, false, cancellationToken: cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {songId} cannot be found");
        }

        if (!_songAuthorizationService.Authorize(song, OperationType.Delete))
        {
            return new ForbiddenAccessError();
        }

        await _uow.SongRepository.DeleteAsync(songId, cancellationToken);
        var fileName = Path.GetFileName(song.Url);

        await _fileHandler.DeleteFileAsync(fileName, FileContainer.Songs, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<SongResponse>> UploadPhotoAsync(int songId, IFormFile photo, CancellationToken cancellationToken = default)
    {
        var song = await _uow.SongRepository.GetByIdAsync(songId, cancellationToken: cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {songId} cannot be found");
        }

        if (!_songAuthorizationService.Authorize(song, OperationType.Patch))
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
        await _uow.SaveChangesAsync();

        return Result.Ok(_mapper.Map<SongResponse>(song));
    }

    public async Task<Result> DeletePhotoAsync(int songId, CancellationToken cancellationToken = default)
    {
        var song = await _uow.SongRepository.GetByIdAsync(songId, cancellationToken: cancellationToken);

        if (song is null)
        {
            return new NotFoundError($"Song with id {songId} cannot be found");
        }

        if (!_songAuthorizationService.Authorize(song, OperationType.Delete))
        {
            return new ForbiddenAccessError();
        }

        if (song.Photo is null)
        {
            return Result.Ok();
        }

        var fileName = Path.GetFileName(song.Photo.URL);
        await _fileHandler.DeleteFileAsync(fileName, FileContainer.Photos, cancellationToken);

        await _uow.PhotoRepository.DeleteAsync(song.Photo.PhotoId);
        await _uow.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<bool> IsSongNameAvailableAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _uow.ExistsAsync<Song>(s => s.Name.ToUpper() == name.ToUpper(), cancellationToken);
    }
}
