using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Infrastructure.Seed;
public static class SeedRoles
{
    public async static Task Seed(RoleManager<IdentityRole> roleManager)
    {
        if (roleManager.Roles.Any()) 
            return;

        var enumRoles = (IEnumerable<Roles>)Enum.GetValues(typeof(Roles));
        var roles = enumRoles.Select(role => new IdentityRole(role.ToString()));

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }
    }
}
