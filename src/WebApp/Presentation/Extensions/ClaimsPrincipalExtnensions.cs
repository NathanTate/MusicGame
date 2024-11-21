using Domain.Entities;
using System.Security.Claims;

namespace Presentation.Extensions;

public static class ClaimsPrincipalExtnensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
    }

    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
    }
}
