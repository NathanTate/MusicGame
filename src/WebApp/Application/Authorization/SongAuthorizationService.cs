using Application.Common.UserHelpers;
using Domain.Entities;
using Domain.Enums;

namespace Application.Authorization;

public interface ISongAuthorizationService
{
    bool Authorize(Song song, OperationType operationType);
}
internal class SongAuthorizationService : ISongAuthorizationService
{
    private readonly IUserContext _userContext;
    public SongAuthorizationService(IUserContext userContext)
    {
        _userContext = userContext;
    }
    public bool Authorize(Song song, OperationType operationType)
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
