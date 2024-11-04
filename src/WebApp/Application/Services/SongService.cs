using Application.DTO.Songs;
using Application.InfrastructureInterfaces;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentResults;

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

        var song = _mapper.Map<Song>(model);

        var songUrl = "https://musicgame.blob.core.windows.net/songs/0036_BreakStride.mp3";
            //await _fileHandler.UploadFileAsync(model.SongFile, FileContainer.Songs, cancellationToken);
        song.Url = songUrl;
        song.Size = model.SongFile.Length;
        song.ContentType = model.SongFile.ContentType;
        song.UserId = userId;

        if (model.PhotoFile is not null)
        {
            var photoUrl = await _fileHandler.UploadFileAsync(model.PhotoFile, FileContainer.Photos, cancellationToken);
            //Todo: upload photo
        }

        var createdSong = _uow.SongRepository.Create(song);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok(_mapper.Map<SongResponse>(createdSong));
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

    public async Task<Result<SongResponse>> GetSongAsync(int songId, CancellationToken cancellationToken = default)
    {
        var song = await _uow.SongRepository.GetByIdAsync(songId, cancellationToken);

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

    public Task<Result<SongResponse>> UpdateSongAsync(UpdateSongRequest model, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
