using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Helpers;
using TechTales.Helpers.Extensions;
using TechTales.Models;
using TechTales.Models.Blog;
using TechTales.Models.Profile;

namespace TechTales.Controllers;

public class ProfileController : Controller
{
    private readonly ILogger<ProfileController> _logger;
    private readonly UserManager<UserEntity> _userManager;
    private readonly AppDbContext _context;

    public ProfileController(ILogger<ProfileController> logger, UserManager<UserEntity> userManager,
        AppDbContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Detail(Guid? id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var currentUserMainRole = await _userManager.GetMainRoleAsync(currentUser);

// If id is null or user is banned return Not Found
        if (id is null || await _context.Bans.AnyAsync(b => b.UserId == id)
            && currentUserMainRole == "User")
        {
            return RedirectToAction("NotFound", "Error");
        }

        var profileUser = await _context.Users
            .AsNoTracking()
            .Include(u => u.Blogs)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (profileUser is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

        var profileUserMainRole = await _userManager.GetMainRoleAsync(profileUser);
        var authorized = await _userManager.IsUserAuthorizedAsync(id.Value, currentUser, "Admin,Moderator");

        var blogs = profileUser.Blogs
            .Where(b => b.Visibility || authorized)
            .Select(b => new BlogViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Content = b.Content,
                CreationDate = b.CreationDate,
                Views = b.Views,
                Visibility = b.Visibility,
            })
            .ToList();

        var ban = await _context.Bans.FirstOrDefaultAsync(b => b.UserId == id);

        var model = new ProfileViewModel
        {
            User = new UserViewModel
            {
                Id = profileUser.Id,
                UserName = profileUser.UserName!,
                Country = profileUser.Country,
                AboutMe = profileUser.AboutMe,
                MainRole = profileUserMainRole,
                Avatar = profileUser.Avatar.BlobToImageSrc(),
                Ban = new BanViewModel
                {
                    EndDate = ban?.BanEndDate.ToString() ?? "none",
                    Reason = ban?.BanReason,
                    State = ban is null ? "Authorized" : "Banned",
                },
            },
            Blogs = blogs,
            IsSameUser = profileUser.Id == currentUser?.Id,
            CurrentUser = new UserViewModel
            {
                MainRole = currentUserMainRole,
            },
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

        var model = new EditProfileViewModel
        {
            UserName = user.UserName!,
            Country = user.Country,
            AboutMe = user.AboutMe,
            Avatar = user.Avatar.BlobToImageSrc(),
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

// Assign user actual avatar to model if some errors will occur
        model.Avatar = user.Avatar.BlobToImageSrc();
        if (!ModelState.IsValid)
        {
            this.ParseModalErrorsAndSet("Validation error");
            return View(model);
        }

        var userByName = await _userManager.FindByNameAsync(model.UserName);
        if (userByName is not null && userByName.Id != user.Id)
        {
            this.SetModalMessage("Error", $"Username '{model.UserName}' already taken");
            return View(model);
        }

        IFormFile? avatar = Request.Form.Files["Avatar"];
        byte[]? newAvatar = null;

        if (avatar != null && avatar.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await avatar.CopyToAsync(memoryStream);

            await CropImageAsync(memoryStream, 200, 200);
            newAvatar = memoryStream.ToArray();
        }

        user.UserName = model.UserName;
        user.Country = model.Country;
        user.AboutMe = model.AboutMe;
        user.Avatar = newAvatar is null ? user.Avatar : newAvatar;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            model.Avatar = newAvatar.BlobToImageSrc();
            this.SetModalMessage("Success", "Your profile has been updated successfully.");
            return View(model);
        }

        this.SetModalMessage("Error", "The server encountered an internal error or misconfiguration and was unable to complete your request.");
        return View(model);
    }

    [HttpGet, Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> List(string? request, string? orderBy, int pageSize = 5, int page = 1)
    {
        IQueryable<UserEntity> query = _context.Users
            .Include(u => u.Blogs)
            .AsNoTracking();

        query = orderBy switch
        {
            "blogsDesc" => query.OrderByDescending(u => u.Blogs.Count),
            "blogsAsc" => query.OrderBy(u => u.Blogs.Count),
            _ => query.OrderByDescending(u => u.Blogs.Count),
        };

        var total = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)total / pageSize);

        var model = new UsersViewModel
        {
            CurrentPage = page,
            TotalPages = totalPages,
        };

        if (!this.IsPageValid(page, totalPages))
        {
            this.SetModalMessage("Invalid page", "No users were found on this page. Page might not exist or was removed.");
            return View(model);
        }

        var list = await query
            .Where(u => u.UserName!.Contains(request ?? string.Empty))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                UserName = u.UserName!,
                CountOfBlogs = u.Blogs.Count,
                Country = u.Country,
                Email = u.Email,
                Avatar = u.Avatar.BlobToImageSrc("/images/default_user_icon.svg"),
                Ban = _context.Bans
                    .Where(b => b.UserId == u.Id)
                    .Select(b => new BanViewModel
                    {
                        EndDate = b.BanEndDate.ToString(),
                        Reason = b.BanReason,
                        State = "Banned",
                    })
                    .FirstOrDefault() ?? new BanViewModel(),
            })
            .ToListAsync();

        model.Users = list;

        return View(model);
    }

    [HttpDelete, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _context.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
        await _context.Bans.Where(b => b.UserId == id).ExecuteDeleteAsync();

        this.SetModalMessage("Delete", $"User with id - '{id}' has been deleted.");
        return Ok($"User with id='{id}' has been deleted.");
    }

    [HttpPost, Authorize(Roles = "Admin,Moderator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUser(Guid id)
    {
        var banEndDate = Request.Form["banEndDate"].ToString();
        var banReason = Request.Form["banReason"].ToString();

        if (string.IsNullOrWhiteSpace(banReason))
        {
            ModelState.AddModelError("Reason", "field is required");
            this.ParseModalErrorsAndSet("Validation error");
            return RedirectToAction("Detail", new { Id = id });
        }

        if (string.IsNullOrWhiteSpace(banEndDate))
        {
            ModelState.AddModelError("End date", "field is required");
            this.ParseModalErrorsAndSet("Validation error");
            return RedirectToAction("Detail", new { Id = id });
        }

        var ban = new BanEntity
        {
            Id = Guid.NewGuid(),
            UserId = id,
            JudgeId = new Guid(_userManager.GetUserId(User)!),
            BanEndDate = DateTime.ParseExact(banEndDate, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            BanReason = banReason,
        };

        await _context.Bans.AddAsync(ban);
        await _context.SaveChangesAsync();

        this.SetModalMessage("Ban", $"User with id - '{id}' has been banned.");
        return RedirectToAction("List", "Profile");
    }

    [HttpPost, Authorize(Roles = "Admin,Moderator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> UnbanUser(Guid id)
    {
        await _context.Bans.Where(b => b.UserId == id).ExecuteDeleteAsync();
        this.SetModalMessage("Unban", $"User with id - '{id}' has been unbanned.");
        return RedirectToAction("List", "Profile");
    }

    private static async Task CropImageAsync(MemoryStream memoryStream, int width, int height)
    {
        memoryStream.Seek(0, SeekOrigin.Begin);

        using var image = Image.Load(memoryStream);
        
// Reduce the image proportionally to a size that allows cropping up to width*height
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Crop
        }));

// Crop the image to width*height pixels
        image.Mutate(x => x.Crop(new Rectangle(0, 0, width, height)));

        memoryStream.SetLength(0);
        await image.SaveAsync(memoryStream, new JpegEncoder());
    }
}