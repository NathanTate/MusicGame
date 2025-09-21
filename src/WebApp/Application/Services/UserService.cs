using Application.Models.Users;
using Application.Errors;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Domain.Interfaces;
using Infrastructure.Context;
using Application.Models.Songs;
using Application.Models;
using Application.Models.Playlists;
using Microsoft.EntityFrameworkCore;
using Application.Models.Queries;
using AutoMapper.QueryableExtensions;
using Application.Services.Elastic;

namespace Application.Services;

internal class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IFileHandler _fileHandler;
    private readonly UserManager<User> _userManager;
    private readonly UsersElasticService _elasticService;
    public UserService(AppDbContext dbContext, IMapper mapper, IFileHandler fileHandler, UserManager<User> userManager, UsersElasticService elasticService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _fileHandler = fileHandler;
        _userManager = userManager;
        _elasticService = elasticService;
    }

    public async Task<PagedList<UserResponse>> GetUsersAsync(UsersQuery query, CancellationToken cancellationToken = default)
    {
        var usersQuery = _dbContext.Users.AsNoTracking();

        List<string> searchIds = new();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var elasticQuery = new ElasticQuery
            {
                PageSize = query.PageSize,
                Page = query.Page,
                SearchTerm = query.SearchTerm
            };

            var result = await _elasticService.SearchAsync(elasticQuery, ElasticIndex.UsersIndex, cancellationToken);

            if (result.IsFailed)
            {
                throw new ArgumentNullException(result.Errors.Select(x => x.Message).FirstOrDefault());
            }

            searchIds = result.Value.Select(x => x.Id).ToList();

            usersQuery = usersQuery.Where(x => searchIds.Contains(x.Id));

            var pagedList = await PagedList<UserResponse>.CreateAsync(
               usersQuery.ProjectTo<UserResponse>(_mapper.ConfigurationProvider),
               query.Page, query.PageSize, cancellationToken);

            var orderedItems = pagedList.Items.OrderBy(x => searchIds.IndexOf(x.UserId)).ToList();

            return new PagedList<UserResponse>(orderedItems, pagedList.Page, pagedList.PageSize, pagedList.TotalCount);
        }

        return await PagedList<UserResponse>.CreateAsync(
            usersQuery.ProjectTo<UserResponse>(_mapper.ConfigurationProvider),
            query.Page, query.PageSize, cancellationToken);
    }

    public async Task<PagedList<UserResponse>> GetByIdsAsync(IEnumerable<string> ids, BaseQuery query, CancellationToken cancellationToken = default)
    {
        var usersQuery = _dbContext.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id));

        return await PagedList<UserResponse>.CreateAsync(
            usersQuery.ProjectTo<UserResponse>(_mapper.ConfigurationProvider),
            query.Page, query.PageSize, cancellationToken);
    }

    public async Task<Result<UserResponse>> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return new NotFoundError($"User with email {email} cannot be found");
        }

        return Result.Ok(_mapper.Map<UserResponse>(user));
    }

    public async Task<Result<UserResponse>> GetUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return new NotFoundError($"User with id {userId} cannot be found");
        }

        return Result.Ok(_mapper.Map<UserResponse>(user));
    }

    public async Task<List<SongResponse>> GetMostPopularSongs(string userId, CancellationToken cancellationToken = default)
    {
        var popularSongs = await _dbContext.Songs
            .AsNoTracking()
            .Where(x => !x.IsPrivate)
            .OrderByDescending(x => x.LikesCount)
            .Take(10).ProjectTo<SongResponse>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

        return popularSongs;
    }

    public async Task<PagedList<PlaylistResponse>> GetUserPlaylists(UserPlaylistsQuery query, string userId, bool own, CancellationToken cancellationToken = default)
    {
        var playlistsQuery = _dbContext.Playlists
            .AsNoTracking()
            .Where(x => x.UserId == userId && (!x.IsPrivate || own));

        if (query.IsPrivate.HasValue && own)
        {
            playlistsQuery = playlistsQuery.Where(x => x.IsPrivate == query.IsPrivate);
        }

        List<int> searchIds = new();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var elasticQuery = new ElasticQuery
            {
                Page = query.Page,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm
            };

            var result = await _elasticService.SearchAsync(elasticQuery, ElasticIndex.PlaylistsIndex, cancellationToken);

            if (result.IsFailed)
            {
                throw new ArgumentNullException(result.Errors.Select(x => x.Message).FirstOrDefault());
            }

            searchIds = result.Value.Select(x => Convert.ToInt32(x.Id)).ToList();

            playlistsQuery = playlistsQuery.Where(x => searchIds.Contains(x.PlaylistId));
        }

        if (query.SortOrder == "desc")
        {
            playlistsQuery = playlistsQuery.OrderByDescending(query.GetSortProperty());
        }
        else
        {
            playlistsQuery = playlistsQuery.OrderBy(query.GetSortProperty());
        }

        var resultQuery = playlistsQuery.Select(x => new PlaylistResponse
        {
            PlaylistId = x.PlaylistId,
            Name = x.Name,
            Description = x.Description,
            IsPrivate = x.IsPrivate,
            TotalDuration = x.TotalDuration,
            LikesCount = x.LikesCount,
            SongsCount = x.Songs.Count,
            CreatedAt = x.CreatedAt,
            PhotoUrl = x.Photo == null ? null : x.Photo.URL,
            User = new ArtistResponse(x.User.Id, x.User.Email, x.User.DisplayName)
        });

        var pagedList = await PagedList<PlaylistResponse>.CreateAsync(resultQuery, query.Page, query.PageSize, cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var orderedItems = pagedList.Items.OrderBy(x => searchIds.IndexOf(x.PlaylistId)).ToList();

            return new PagedList<PlaylistResponse>(orderedItems, pagedList.Page, pagedList.PageSize, pagedList.TotalCount);
        }

        return pagedList;
    }

    public async Task<PagedList<PlaylistResponse>> GetLikedPlaylists(UserPlaylistsQuery query, string userId, CancellationToken cancellationToken = default)
    {
        var likedPlaylistsQuery = _dbContext.PlaylistLike
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.Playlist.IsPrivate)
            .Select(x => x.Playlist);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            likedPlaylistsQuery = likedPlaylistsQuery.Where(x =>
                x.Name.ToLower().Contains(query.SearchTerm.ToLower()) ||
                x.Description != null && x.Description.ToLower().Contains(query.SearchTerm.ToLower()));
        }

        if (query.SortOrder == "desc")
        {
            likedPlaylistsQuery = likedPlaylistsQuery.OrderByDescending(query.GetSortProperty());
        }
        else
        {
            likedPlaylistsQuery = likedPlaylistsQuery.OrderBy(query.GetSortProperty());
        }

        var likedResultQuery = likedPlaylistsQuery.Select(x => new PlaylistResponse
        {
            PlaylistId = x.PlaylistId,
            Name = x.Name,
            Description = x.Description,
            IsPrivate = x.IsPrivate,
            PhotoUrl = x.Photo == null ? null : x.Photo.URL,
            User = new ArtistResponse(x.User.Id, x.User.Email, x.User.DisplayName)
        });

        return await PagedList<PlaylistResponse>.CreateAsync(likedResultQuery, query.Page, query.PageSize, cancellationToken);
    }

    public async Task<PagedList<SongResponse>> GetUsersSongs(UserSongsQuery query, string userId, bool own, CancellationToken cancellationToken = default)
    {
        var songsQuery = _dbContext.Songs
            .AsNoTracking()
            .Where(x => x.UserId == userId && (!x.IsPrivate || own));

        if (query.IsPrivate.HasValue && own)
        {
            songsQuery = songsQuery.Where(x => x.IsPrivate == query.IsPrivate);
        }

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

    public async Task<PagedList<SongResponse>> GetLikedSongs(UserSongsQuery query, string userId, CancellationToken cancellationToken = default)
    {
        var likedSongsQuery = _dbContext.SongLike
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.Song.IsPrivate)
            .Select(x => x.Song);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            likedSongsQuery = likedSongsQuery.Where(x =>
                x.Name.ToLower().Contains(query.SearchTerm.ToLower()) ||
                x.User.DisplayName.ToLower().Contains(query.SearchTerm.ToLower()) ||
                x.Genres.Any(x => x.NormalizedName.Contains(query.SearchTerm.ToUpper())));
        }

        if (query.SortOrder == "desc")
        {
            likedSongsQuery = likedSongsQuery.OrderByDescending(query.GetSortProperty());
        }
        else
        {
            likedSongsQuery = likedSongsQuery.OrderBy(query.GetSortProperty());
        }

        return await PagedList<SongResponse>.CreateAsync(likedSongsQuery.ProjectTo<SongResponse>(_mapper.ConfigurationProvider), query.Page, query.PageSize);
    }
    public async Task<Result<UserResponse>> UpdateUserProfileAsync(UpdateProfileRequest model, string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);


        if (user == null)
        {
            return new NotFoundError($"User with id {userId} cannot be found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        user.Roles = roles.ToList();
        user.Email = model.Email;
        user.DisplayName = model.Username;

        if (model.Photo is not null)
        {
            var photoUrl = user.Photo is null
            ? await _fileHandler.UploadFileAsync(model.Photo, FileContainer.Photos, cancellationToken)
            : await _fileHandler.UpdateFileAsync(Path.GetFileName(user.Photo.URL), model.Photo, FileContainer.Photos, cancellationToken);

            var userPhoto = new Photo()
            {
                URL = photoUrl,
                Size = model.Photo.Length,
                ContentType = model.Photo.ContentType,
            };

            user.Photo = userPhoto;
            await _dbContext.SaveChangesAsync();
        }

        return Result.Ok(_mapper.Map<UserResponse>(user));
    }
}
