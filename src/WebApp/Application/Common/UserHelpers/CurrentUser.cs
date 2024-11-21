namespace Application.Common.UserHelpers;
public sealed record CurrentUser(string UserId, string Email, IEnumerable<string> Roles)
{
    public bool isInRole(string role) => Roles.Contains(role);
}
