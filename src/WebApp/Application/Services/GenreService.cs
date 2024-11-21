﻿using Application.DTO.Genres;
using Application.Errors;
using Application.InfrastructureInterfaces;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentResults;

namespace Application.Services;
internal class GenreService : IGenreService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    public GenreService(IUnitOfWork uow, IMapper mapper)
    {
        _mapper = mapper;
        _uow = uow;
    }

    public async Task<List<GenreResponse>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        var genres = await _uow.GenreRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<List<GenreResponse>>(genres);
    }

    public async Task<Result<GenreResponse>> GetGenreAsync(int genreId, CancellationToken cancellationToken = default)
    {
        var genre = await _uow.GenreRepository.GetByIdAsync(genreId, false, cancellationToken);

        if (genre is null)
        {
            return new NotFoundError($"Genre with id {genreId} cannot be found");
        }

        return Result.Ok(_mapper.Map<GenreResponse>(genre));
    }

    public async Task<Result<GenreResponse>> CreateGenreAsync(CreateGenreRequest model, CancellationToken cancellationToken = default)
    {
        var exists = await _uow.ExistsAsync<Genre>(g => g.NormalizedName == model.Name.ToUpper(), cancellationToken);

        if (exists)
        {
            return new ValidationError($"Genre with name {model.Name} already exists");
        }

        var genre = _mapper.Map<Genre>(model);

        _uow.GenreRepository.Create(genre);

        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok(_mapper.Map<GenreResponse>(genre));
    }

    public async Task<Result<GenreResponse>> UpdateGenreAsync(UpdateGenreRequest model, CancellationToken cancellationToken = default)
    {
        var genre = await _uow.GenreRepository.GetByIdAsync(model.GenreId, true, cancellationToken);

        if (genre is null)
        {
            return new NotFoundError($"Genre with id {model.GenreId} cannot be found");
        }

        _mapper.Map(model, genre);

        _uow.GenreRepository.Update(genre);

        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok(_mapper.Map<GenreResponse>(genre));
    }

    public async Task<Result> DeleteGenreAsync(int genreId, CancellationToken cancellationToken = default)
    {
        var success = await _uow.GenreRepository.DeleteAsync(genreId, cancellationToken);

        if (!success)
        {
            return new NotFoundError($"Genre with id {genreId} cannot be found");
        }

        await _uow.SaveChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}
