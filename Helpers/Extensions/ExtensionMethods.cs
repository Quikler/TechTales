using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using TechTales.Data.Models;

namespace TechTales.Helpers.Extensions;

public static class ExtensionMethods
{
    public static string BlobToImageSrc(this byte[]? bytes, 
        string defaultImage = "/images/default_user_icon.svg")
    {
        string base64String = bytes != null && bytes.Length > 0 
            ? Convert.ToBase64String(bytes) 
            : string.Empty;

        string imageSrc = !string.IsNullOrEmpty(base64String) 
            ? $"data:image/png;base64,{base64String}" 
            : defaultImage;

        return imageSrc;
    }

    public static string EllipsisString(this string input, int end, int maxParagraphs = 4)
    {
        var paragraphs = input.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);

        if (paragraphs.Length > maxParagraphs)
        {
            var biggestThan = paragraphs.Count(s => s.Length >= 60);

            if (biggestThan >= 2)
            {
                input = string.Join("\r\n", paragraphs.Take(2)) + "...";
            }
            else
            {
                input = string.Join("\r\n", paragraphs.Take(maxParagraphs)) + "...";
            }
            //return input;
        }

        if (input.Length > end)
        {
            input = string.Concat(input[..end], "...");
        }
        return input;
    }

    public static async Task InitializeRolesAsync(this IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        string[] roleNames = { "Admin", "Moderator", "User" };
        IdentityResult roleResult;

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    public static async Task<string> GetMainRoleAsync(this UserManager<UserEntity> userManager, UserEntity? user)
    {
        var role = "User";
        if (user is null) return role;

        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            role = "Admin";
        }
        else if (await userManager.IsInRoleAsync(user, "Moderator"))
        {
            role = "Moderator";
        }
        return role;
    }
}