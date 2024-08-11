using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Models;

public class BlogController : Controller
{
    private readonly ILogger<BlogController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<UserEntity> _userManager;

    public BlogController(ILogger<BlogController> logger, AppDbContext context,
        UserManager<UserEntity> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Blog()
    {
        return View();
    }

    public IActionResult Edit()
    {
        return View();
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(BlogViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var tags = model.Tags?
            .Split('#', StringSplitOptions.RemoveEmptyEntries)
            .Select(tagStr => new TagEntity { Name = tagStr.Trim() })
            .Where(tag => !string.IsNullOrWhiteSpace(tag.Name))
            .ToList();
        
        var categories = model.Categories?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(categoryStr => new CategoryEntity { Name = categoryStr.Trim() })
            .Where(category => !string.IsNullOrWhiteSpace(category.Name))
            .ToList();

        var blogEntity = new BlogEntity
        {
            Title = model.Title,
            Content = model.Content,
            Visibility = model.Visibility,
            Tags = tags ?? [],
            Catogories = categories ?? [],
            Author = user,
        };

        await _context.Blogs.AddAsync(blogEntity);
        await _context.SaveChangesAsync();

        return RedirectToAction("Detail", "Profile");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
