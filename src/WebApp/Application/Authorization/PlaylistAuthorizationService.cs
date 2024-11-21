using Application.Common.UserHelpers;
using Domain.Entities;
using Domain.Enums;

namespace Application.Authorization;

public interface IPlaylistAuthorizationService
{
    bool Authorize(Playlist song, OperationType operationType);
}
internal class PlaylistAuthorizationService : IPlaylistAuthorizationService
{
    private readonly IUserContext _userContext;
    public PlaylistAuthorizationService(IUserContext userContext)
    {
        _userContext = userContext;
    }
    public bool Authorize(Playlist song, OperationType operationType)
    {
        CurrentUser? user = _userContext.GetCurrentUser();

        if (operationType == OperationType.Create || operationType == OperationType.Read)
        {
            return true;
        }

        if (user is null)
        {
            return false;
        }

        if ((operationType == OperationType.Delete || operationType == OperationType.Update || operationType == OperationType.Patch)
            && (user.isInRole(nameof(Role.ADMIN)) || song.UserId == user.UserId))
        {
            return true;
        }

        return false;
    }
}
