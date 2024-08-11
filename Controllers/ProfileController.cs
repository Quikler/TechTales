using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using TechTales.Data;
using TechTales.Data.Models;
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

    public async Task<IActionResult> Detail()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        var model = new ProfileViewModel
        {
            UserName = user.UserName,
            Country = user.Country,
            AboutMe = user.AboutMe,
            Avatar = user.Avatar,
        };
        return View(model);
    }

    public async Task<IActionResult> Edit() 
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
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
            return NotFound();
        }

        // Assign user actual avatar to model if some errors will occur
        model.Avatar = user.Avatar;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userByName = await _userManager.FindByNameAsync(model.UserName);
        if (userByName is not null && userByName.Id != user.Id)
        {
            ModelState.AddModelError(string.Empty, $"User with '{model.UserName}' username already exists");
            return View(model);
        }

        if (avatar != null && avatar.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await avatar.CopyToAsync(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            // Uploading an image using ImageSharp
            using var image = Image.Load(memoryStream);
            // Reduce the image proportionally to a size that allows cropping up to 400x400.
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(400, 400),
                Mode = ResizeMode.Crop
            };
            image.Mutate(x => x.Resize(resizeOptions));

            // Crop the image to 400x400 pixels
            image.Mutate(x => x.Crop(new Rectangle(0, 0, 400, 400)));

            memoryStream.SetLength(0);
            await image.SaveAsync(memoryStream, new JpegEncoder());

            model.Avatar = memoryStream.ToArray();
        }

        user.UserName = model.UserName;
        user.Country = model.Country;
        user.AboutMe = model.AboutMe;
        user.Avatar = model.Avatar is null ? user.Avatar : model.Avatar;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("Detail", "Profile");
        }

        ModelState.AddModelError(string.Empty, "An error occurred while updating the profile.");

        return View(model);
    }

    // [HttpPost]
    // public async Task<IActionResult> Edit(EditProfileViewModel model, IFormFile? avatar)
    // {
    //     if (!ModelState.IsValid)
    //     {
    //         return View(model);
    //     }

    //     if (avatar is not null && avatar.Length > 0)
    //     {
    //         using var memoryStream = new MemoryStream();
    //         await avatar.CopyToAsync(memoryStream);
    //         model.Avatar = memoryStream.ToArray();
    //     }

    //     var user = await _userManager.GetUserAsync(User);

    //     if (user is not null)
    //     {
    //         user.UserName = model.UserName;
    //         user.Country = model.Country;
    //         user.AboutMe = model.AboutMe;
    //         user.Avatar = model.Avatar is null ? user.Avatar : model.Avatar;
            
    //         var result = await _userManager.UpdateAsync(user);
    //         if (result.Succeeded)
    //         {
    //             return RedirectToAction("Detail", "Profile");
    //         }
    //     }

    //     ModelState.AddModelError("", "An error occurred while updating the profile.");

    //     return View(model);
    // }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
