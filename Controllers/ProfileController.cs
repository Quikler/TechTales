using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Helpers;
using TechTales.Helpers.Extensions;
using TechTales.Models;

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
        if (id is null || id == Guid.Empty)
        {
            return RedirectToAction("Login", "Authorization");
        }

        var profileUser = await _context.Users
            .AsNoTracking()
            .Include(u => u.Blogs)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (profileUser is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

        var currentUser = await _userManager.GetUserAsync(User);

        var role = await _userManager.GetMainRoleAsync(profileUser);
        var model = new ProfileViewModel
        {
            User = new UserViewModel
            {
                MainRole = role,
                UserName = profileUser.UserName!,
                Country = profileUser.Country,
                AboutMe = profileUser.AboutMe,
                Avatar = profileUser.Avatar.BlobToImageSrc()
            },
            Blogs = profileUser.Blogs.Select(b => new BlogViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Content = b.Content,
                CreationDate = b.CreationDate, 
                Views = b.Views,
            }).ToList(),
            IsSameUser = profileUser.Id == currentUser?.Id,
            CurrentUser = new UserViewModel
            {
                MainRole = await _userManager.GetMainRoleAsync(currentUser),
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
            Avatar = user.Avatar,
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditProfileViewModel model, IFormFile? avatar)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

// Assign user actual avatar to model if some errors will occur
        model.Avatar = user.Avatar;
        if (!ModelState.IsValid)
        {
            this.ParseModalErrorsAndSet("Validation error");
            return View(model);
        }

        var userByName = await _userManager.FindByNameAsync(model.UserName);
        if (userByName is not null && userByName.Id != user.Id)
        {
            this.SetModalMessage("Coincidence error", $"Username '{model.UserName}' already taken");
            return View(model);
        }

        if (avatar != null && avatar.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await avatar.CopyToAsync(memoryStream);

            await CropImageAsync(memoryStream, 400, 400);
            model.Avatar = memoryStream.ToArray();
        }

        user.UserName = model.UserName;
        user.Country = model.Country;
        user.AboutMe = model.AboutMe;
        user.Avatar = model.Avatar is null ? user.Avatar : model.Avatar;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            this.SetModalMessage("Success", "Your profile has been updated successfully.");
            return View(model);
        }

        this.SetModalMessage("Error", "The server encountered an internal error or misconfiguration and was unable to complete your request.");
        return View(model);
    }

    [HttpDelete, Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _context.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
        this.SetModalMessage("Success", $"User with id='{id}' has been deleted.");
        return View();
    }

    [HttpPost, Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> BanUser(Guid id)
    {
        return View();
    }

    private static async Task CropImageAsync(MemoryStream memoryStream, int width, int height)
    {
        memoryStream.Seek(0, SeekOrigin.Begin);

// Uploading an image using ImageSharp
        using var image = Image.Load(memoryStream);
// Reduce the image proportionally to a size that allows cropping up to 400x400.
        var resizeOptions = new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Crop
        };
        image.Mutate(x => x.Resize(resizeOptions));

// Crop the image to 400x400 pixels
        image.Mutate(x => x.Crop(new Rectangle(0, 0, width, height)));

        memoryStream.SetLength(0);
        await image.SaveAsync(memoryStream, new JpegEncoder());
    }
}