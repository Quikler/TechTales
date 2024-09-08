using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using TechTales.Data.Models;

namespace TechTales.Helpers.Extensions;

public static class UserManagerExtenstions
{
    public static async Task<bool> IsUserAuthorizedAsync(this UserManager<UserEntity> userManager,
        Guid authorId, ClaimsPrincipal user, string roles = "Admin")
    {
        var currentUser = await userManager.GetUserAsync(user);
        var currentUserMainRole = await userManager.GetMainRoleAsync(currentUser);

        var rolesArr = roles.Split(',');

        return rolesArr.Any(r => r == currentUserMainRole) || authorId == currentUser?.Id;
    }

    public static async Task<bool> IsUserAuthorizedAsync(this UserManager<UserEntity> userManager,
        Guid authorId, UserEntity? user, string roles = "Admin")
    {
        var currentUserMainRole = await userManager.GetMainRoleAsync(user);
        return roles.Split(',').Any(r => r == currentUserMainRole) || authorId == user?.Id;
    }
}