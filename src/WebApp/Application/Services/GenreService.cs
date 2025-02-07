using Application.Models.Genres;
using Application.Errors;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentResults;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using Application.Models.Queries;
using Application.Models;
using Domain.Enums;
using Application.Services.Elastic;

namespace Application.Services;
internal class GenreService : IGenreService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly GenresElasticService _elasticService;
    public GenreService(AppDbContext dbContext, IMapper mapper, GenresElasticService elasticService)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _elasticService = elasticService;
    }

    public async Task<PagedList<GenreResponse>> GetGenresAsync(GenresQuery query, CancellationToken cancellationToken = default)
    {
        var genresQuery = _dbContext.Genres
            .AsNoTracking();

        if (query.IsSystemDefined is not null)
        {
            genresQuery = genresQuery.Where(x => x.IsSystemDefined == query.IsSystemDefined);
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

            var result = await _elasticService.SearchAsync(elasticQuery, ElasticIndex.GenresIndex);

            if (result.IsFailed)
            {
                throw new ArgumentNullException(result.Errors.Select(x => x.Message).FirstOrDefault());
            }

            searchIds = result.Value.Select(x => Convert.ToInt32(x.Id)).ToList();
            
            genresQuery = genresQuery.Where(x => searchIds.Contains(x.GenreId));
        }

        if (query.SortOrder == "desc")
        {
            genresQuery = genresQuery.OrderByDescending(x => x.Name);
        }
        else
        {
            genresQuery = genresQuery.OrderBy(x => x.Name);
        }

        var pagedList = await PagedList<GenreResponse>.CreateAsync(genresQuery.ProjectTo<GenreResponse>(_mapper.ConfigurationProvider), query.Page, query.PageSize);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var orderedItems = pagedList.Items.OrderBy(x => searchIds.IndexOf(x.GenreId)).ToList();

            return new PagedList<GenreResponse>(orderedItems, pagedList.Page, pagedList.PageSize, pagedList.TotalCount);
        }

        return pagedList;
    }

    public async Task<PagedList<GenreResponse>> GetByIdsAsync(IEnumerable<int> ids, GenresQuery query, CancellationToken cancellationToken = default)
    {
        var genresQuery = _dbContext.Genres
           .AsNoTracking()
           .Where(x => ids.Contains(x.GenreId));

        if (query.IsSystemDefined is not null)
        {
            genresQuery = genresQuery.Where(x => x.IsSystemDefined == query.IsSystemDefined);
        }

        return await PagedList<GenreResponse>.CreateAsync(genresQuery.ProjectTo<GenreResponse>(_mapper.ConfigurationProvider), query.Page, query.PageSize);
    }

    public async Task<Result<GenreResponse>> GetGenreAsync(int genreId, CancellationToken cancellationToken = default)
    {
        var genre = await _dbContext.Genres.FindAsync([genreId], cancellationToken);

        if (genre is null)
        {
            return new NotFoundError($"Genre with id {genreId} cannot be found");
        }

        return Result.Ok(_mapper.Map<GenreResponse>(genre));
    }

    public async Task<Result<GenreResponse>> CreateGenreAsync(CreateGenreRequest model, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Genres.AnyAsync(g => g.NormalizedName == model.Name.ToUpper(), cancellationToken);

        if (exists)
        {
            return new ValidationError($"Genre with name {model.Name} already exists");
        }

        var genre = _mapper.Map<Genre>(model);

        _dbContext.Genres.Add(genre);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(_mapper.Map<GenreResponse>(genre));
    }

    public async Task<Result<GenreResponse>> UpdateGenreAsync(UpdateGenreRequest model, CancellationToken cancellationToken = default)
    {
        var genre = await _dbContext.Genres.FindAsync([model.GenreId], cancellationToken);

        if (genre is null)
        {
            return new NotFoundError($"Genre with id {model.GenreId} cannot be found");
        }

        _mapper.Map(model, genre);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(_mapper.Map<GenreResponse>(genre));
    }

    public async Task<Result> DeleteGenreAsync(int genreId, CancellationToken cancellationToken = default)
    {
        var genre = await _dbContext.Genres.FindAsync([genreId], cancellationToken);

        if (genre is null)
        {
            return new NotFoundError($"Genre with id {genreId} cannot be found");
        }

        _dbContext.Remove(genre);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<bool> IsGenreNameAvailable(string name, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Genres.AnyAsync(p => p.Name.ToUpper() == name.ToUpper(), cancellationToken);
        return !exists;
    }

}
