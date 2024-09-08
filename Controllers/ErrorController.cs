
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Models;

namespace TechTales.Controllers;

public class ErrorController : Controller
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly AppDbContext _context;

    public ErrorController(UserManager<UserEntity> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public new IActionResult NotFound()
    {
        return View();
    }

    public IActionResult InternalServer()
    {
        return View();
    }

    public IActionResult Forbidden()
    {
        return View();
    }

    public async Task<IActionResult> Banned()
    {
        var userId = _userManager.GetUserId(User);
        var ban = await _context.Bans.FirstOrDefaultAsync(b => b.UserId.ToString() == userId);

        var model = new ErrorViewModel
        {
            Reason = ban?.BanReason,
            BanEndDate = ban?.BanEndDate.ToString("dd/MM/yyyy HH:mm:ss"),
        };

        return View(model);
    }
}