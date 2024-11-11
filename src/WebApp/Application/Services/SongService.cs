using Application.DTO.Songs;
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
    public SongService(IUnitOfWork uow, IMapper mapper, IFileHandler fileHandler)
    {
        _uow = uow;
        _mapper = mapper;
        _fileHandler = fileHandler;
    }

    public async Task<Result<SongResponse>> CreateSongAsync(CreateSongRequest model, string userId, CancellationToken cancellationToken = default)
    {
        var exists = await _uow.ExistsAsync<Song>(s => s.Name.ToUpper() == model.Name.ToUpper(), cancellationToken);

        if (exists)
        {
            return Result.Fail($"Song with name {model.Name} arleady exists");
        }

        var allExist = model.GenreIds.TrueForAll(id => _uow.Exists<Genre>(e => e.GenreId == id));

        if (!allExist)
        {
            return Result.Fail($"Please provide valid genres, one of provided does not exist");
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

    public async Task<Result<SongResponse>> GetSongAsync(int songId, CancellationToken cancellationToken = default)
    {
        var song = await _uow.SongRepository.GetByIdAsync(songId, false, cancellationToken);

        if (song is null)
        {
            return Result.Fail($"Song with Id - {songId} doesn't exist");
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
        var nameExists = await _uow.ExistsAsync<Song>(x => x.Name.ToUpper() == model.Name.ToUpper(), cancellationToken);

        if (nameExists)
        {
            return Result.Fail($"Song with name {model.Name} already exists");
        }

        var song = await _uow.SongRepository.GetByIdAsync(model.SongId, true, cancellationToken);

        if (song is null)
        {
            return Result.Fail($"Song with Id - {model.SongId} doesn't exist");
        }

        _mapper.Map(model, song);

        _uow.SongRepository.AttachGenresToSong(song, model.GenreIds);
        _uow.SongRepository.Update(song);

        await _uow.SaveChangesAsync();

        return Result.Ok(_mapper.Map<SongResponse>(song));
    }

    public async Task<Result> DeleteSongAsync(int songId, CancellationToken cancellationToken = default)
    {
        var songUrl = await _uow.SongRepository.DeleteAsync(songId, cancellationToken);

        if (songUrl is null)
        {
            return Result.Fail($"Song with Id - {songId} doesn't exist");
        }

        var fileName = Path.GetFileName(songUrl);

        await _fileHandler.DeleteFileAsync(fileName, FileContainer.Songs, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<SongResponse>> UploadPhotoAsync(int songId, IFormFile photo, CancellationToken cancellationToken = default)
    {
        var song = await _uow.SongRepository.GetByIdAsync(songId, true, cancellationToken);

        if (song is null)
        {
            return Result.Fail($"Song with Id - {songId} doesn't exist");
        }

        var photoUrl = song.Photo is null 
            ? await _fileHandler.UploadFileAsync(photo, FileContainer.Photos, cancellationToken)
            : await _fileHandler.UpdateFileAsync(Path.GetFileName(song.Url), photo, FileContainer.Photos, cancellationToken);

        Photo songPhoto = new Photo()
        {
            URL = photoUrl,
            Size = photo.Length,
            ContentType = photo.ContentType,
        };

        song.Photo = songPhoto;
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok(_mapper.Map<SongResponse>(song));
    }

    public async Task<Result> DeletePhotoAsync(int songId, CancellationToken cancellationToken = default)
    {
        var song = await _uow.SongRepository.GetByIdAsync(songId, true, cancellationToken);

        if (song is null)
        {
            return Result.Fail($"Song with Id - {songId} doesn't exist");
        }

        if (song.Photo is null)
        {
            return Result.Fail($"There is not photo to be deleted");
        }

        var fileName = Path.GetFileName(song.Photo.URL);
        await _fileHandler.DeleteFileAsync(fileName, FileContainer.Photos, cancellationToken);

        await _uow.PhotoRepository.DeleteAsync(song.Photo.PhotoId);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
