using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Helpers;
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

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 3; // Number of blogs on one page
        var totalBlogs = await _context.Blogs.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);
        
        var blogs = await _context.Blogs
            .AsNoTracking()
            .OrderByDescending(b => b.Views)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(b => b.Author)
            .Select(b => new BlogViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Content = b.Content,
                CreationDate = b.CreationDate,
                Visibility = b.Visibility,
                Views = b.Views,
                Tags = string.Join(' ', b.Tags.Select(t => t.Name)),
                Categories = string.Join(' ', b.Categories.Select(c => c.Name)),
                Author = new UserViewModel
                {
                    Id = b.AuthorId,
                    UserName = b.Author.UserName!,
                    AboutMe = b.Author.AboutMe,
                    Country = b.Author.Country,
                    Avatar = b.Author.Avatar.BlobToImageSrc("/images/default_user_icon.svg"),
                }
            })
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

