using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Helpers;
using TechTales.Helpers.Extensions;
using TechTales.Models;
using TechTales.Models.Home;
using Telegram.Bot;

namespace TechTales.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<UserEntity> _userManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, UserManager<UserEntity> userManager,
        AppDbContext context, IConfiguration configuration)
    {
        _logger = logger;
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 3; // Number of blogs on one page
        var totalBlogs = await _context.Blogs.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

        var model = new HomeViewModel
        {
            CurrentPage = page,
            TotalPages = totalPages
        };

        if (page <= 0 || page > totalPages)
        {
            this.SetModalMessage("Invalid page", "No blogs was found on this page. Page might not exist or was removed.");
            return View(model);
        }

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
                Views = b.Views,
                Author = new UserViewModel
                {
                    Id = b.AuthorId,
                    UserName = b.Author.UserName!,
                    Avatar = b.Author.Avatar.BlobToImageSrc("/images/default_user_icon.svg"),
                }
            })
            .ToListAsync();

        model.Blogs = blogs;

        return View(model);
    }

    [HttpGet]
    public IActionResult About()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Tags(int page = 1)
    {
        int pageSize = 10; // Number of tags on one page
        var totalCategories = await _context.Tags.CountAsync(t => t.Blogs.Count > 0);
        var totalPages = (int)Math.Ceiling((double)totalCategories / pageSize);

        var model = new TagsViewModel
        {
            CurrentPage = page,
            TotalPages = totalPages
        };

        if (page <= 0 || page > totalPages)
        {
            this.SetModalMessage("Invalid page", "No tags was found on this page. Page might not exist or was removed.");
            return View(model);
        }

        var tags = await _context.Tags
            .Where(t => t.Blogs.Count > 0)
            .OrderByDescending(c => c.Blogs.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Blogs)
            .Select(c => new TagViewModel
            {
                Name = c.Name,
                CountOfBlogs = c.Blogs.Count,
            })
            .ToListAsync();

        model.Tags = tags;

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Categories(int page = 1)
    {
        int pageSize = 10; // Number of categories on one page
        var totalCategories = await _context.Categories.CountAsync(c => c.Blogs.Count > 0);
        var totalPages = (int)Math.Ceiling((double)totalCategories / pageSize);

        var model = new CategoriesViewModel
        {
            CurrentPage = page,
            TotalPages = totalPages
        };

        if (page <= 0 || page > totalPages)
        {
            this.SetModalMessage("Invalid page", "No categories was found on this page. Page might not exist or was removed.");
            return View(model);
        }

        var categories = await _context.Categories
            .Where(c => c.Blogs.Count > 0)
            .OrderByDescending(c => c.Blogs.Count)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Blogs)
            .Select(c => new CategoryViewModel
            {
                Name = c.Name,
                CountOfBlogs = c.Blogs.Count,
            })
            .ToListAsync();

        model.Categories = categories;

        return View(model);
    }

    [HttpGet]
    public IActionResult Contacts()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Contacts(ContactsViewModel model)
    {
        // ViewBag.ModalTitle = "Test";
        // ViewBag.ModalContent = "Test";
        // return View();

        if (!ModelState.IsValid)
        {
            this.ParseModalErrorsAndSet("Validation error");
            return View();
        }

// TgBot:Token is telegram bot token located in user-secrets
        string? token = _configuration["TgBot:Token"];
        string? adminId = _configuration["TelegramAdminId"];

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(adminId))
        {
            this.SetModalMessage("Configuration error", "We encountered an issue while loading the application configuration. It appears that some necessary configuration files are missing from the server.");
            return View();
        }

        try
        {
            var bot = new TelegramBotClient(token);
            await bot.SendTextMessageAsync(adminId, $"Name: {model.Name}\nEmail: {model.Email}\nMessage: {model.Message}");
        }
        catch
        {
            this.SetModalMessage("Error", "The server encountered an internal error or misconfiguration and was unable to complete your request.");
            return View();
        }

        this.SetModalMessage("Message", "Thank you! The message was sent to the administration successfully.");
        return View();
    }
}