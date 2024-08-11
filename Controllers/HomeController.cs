using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Models;

namespace TechTales.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<UserEntity> _userManager;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, UserManager<UserEntity> userManager,
        AppDbContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 3; // Number of blogs on one page
        var totalBlogs = await _context.Blogs.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

        // if (page <= 0) page = 1;
        // else if (page >= totalPages) page = totalPages;
        
        var blogs = await _context.Blogs
            .AsNoTracking()
            .OrderByDescending(b => b.Content.Length)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(b => b.Author)
            .ToListAsync();

        var model = new HomeViewModel
        {
            Blogs = blogs,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

