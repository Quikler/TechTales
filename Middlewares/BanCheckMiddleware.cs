using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechTales.Data;
using TechTales.Data.Models;

namespace TechTales.Middlewares;

public class BanCheckMiddleware
{
    private readonly RequestDelegate _next;

    public BanCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, UserManager<UserEntity> userManager, AppDbContext dbContext)
    {
        // Allow access to specific paths
        var allowedPaths = new[] { "/Error/Banned", "/Error/NotFound", "/Error/Forbidden", "/Error/InternalServer" };
        if (allowedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
        {
            await _next(context);
            return;
        }

        var userId = userManager.GetUserId(context.User);
        var ban = await dbContext.Bans.FirstOrDefaultAsync(b => b.UserId.ToString() == userId);
        
        if (ban is not null && ban.BanEndDate > DateTime.UtcNow)
        {
            // Redirect to the Banned page
            context.Response.Redirect("/Error/Banned");
            return;
        }

        await _next(context);
    }
}