using Application.DTO.Playlists;
using Application.InfrastructureInterfaces;
using Application.Interfaces;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Services;
internal class PlaylistService : IPlaylistService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileHandler _fileHandler;
    public PlaylistService(IUnitOfWork uow, IMapper mapper, IFileHandler fileHandler)
    {
        _uow = uow;
        _mapper = mapper;
        _fileHandler = fileHandler;
    }

    public Task<Result> CreatePlaylistAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeletePhotoAsync(int songId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeletePlaylistAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> GetPlaylistAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> GetPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> UpdatePlaylistAsync(UpdatePlaylistRequest model, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PlaylistResponse>> UploadPhotoAsync(int songId, IFormFile photo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
