using Application.Models;
using Application.Models.Playlists;
using Application.Models.Queries;
using Application.Models.Songs;
using Application.Models.Users;
using FluentResults;

namespace Application.Interfaces;
public interface IUserService
{
    Task<PagedList<UserResponse>> GetUsersAsync(UsersQuery query, CancellationToken cancellationToken = default);
    Task<PagedList<UserResponse>> GetByIdsAsync(IEnumerable<string> ids, BaseQuery query, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> GetUserByEmailAsync(string email);
    Task<Result<UserResponse>> GetUserAsync(string userId);
    Task<Result<UserResponse>> UpdateUserProfileAsync(UpdateProfileRequest model, string userId, CancellationToken cancellationToken = default);
    Task<PagedList<PlaylistResponse>> GetUserPlaylists(UserPlaylistsQuery query, string userId, bool own, CancellationToken cancellationToken = default);
    Task<PagedList<PlaylistResponse>> GetLikedPlaylists(UserPlaylistsQuery query, string userId, CancellationToken cancellationToken = default);
    Task<PagedList<SongResponse>> GetUsersSongs(UserSongsQuery query, string userId, bool own, CancellationToken cancellationToken = default);
    Task<PagedList<SongResponse>> GetLikedSongs(UserSongsQuery query, string userId, CancellationToken cancellationToken = default);
    Task<List<SongResponse>> GetMostPopularSongs(string userId, CancellationToken cancellationToken = default);
}
