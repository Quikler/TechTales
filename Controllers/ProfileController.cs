using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechTales.Data.Models;
using TechTales.Models;

namespace TechTales.Controllers;

public class ProfileController : Controller
{
    private readonly ILogger<ProfileController> _logger;
    private readonly UserManager<UserEntity> _userManager;

    public ProfileController(ILogger<ProfileController> logger, UserManager<UserEntity> userManager)
    {
        _logger = logger;
        _userManager = userManager;
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

        var model = new ProfileViewModel
        {
            UserName = user.UserName,
            Country = user.Country,
            AboutMe = user.AboutMe,
            Avatar = user.Avatar,
        };
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
