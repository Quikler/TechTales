using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Helpers.Extensions;
using TechTales.Models;
using TechTales.Models.Category;
using TechTales.Models.Home;

namespace TechTales.Controllers;

public class CategoryController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<UserEntity> _userManager;

    public CategoryController(ILogger<HomeController> logger, AppDbContext context, UserManager<UserEntity> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> List(string? request, string? orderBy, int pageSize = 10, int page = 1)
    {
        IQueryable<CategoryEntity> query = _context.Categories.AsNoTracking();

        var currentUserMainRole = await _userManager.GetMainRoleAsync(User);
        if (currentUserMainRole == "User")
        {
            query = query
                .Include(c => c.Blogs)
                .ThenInclude(b => b.Author)
                .Where(c => c.Blogs.Any(b => b.Visibility && !_context.Bans.Any(bn => bn.UserId == b.AuthorId)));
        }
        else
        {
            query = query.Include(c => c.Blogs);
        }

        if (request is not null)
        {
            var entityNames = request.Split(new char[] { ' ', '#', ',', '.', '|' }, 
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            query = query
                .AsEnumerable() // Switch to client processing
                .Where(c => entityNames.Any(e => c.Name.ToLower().Contains(e.ToLower())))
                .AsQueryable();
        }

        query = orderBy switch
        {
            "AlphabeticalDesc" => query.OrderByDescending(c => c.Name),
            "AlphabeticalAsc" => query.OrderBy(c => c.Name),
            "LengthDesc" => query.OrderByDescending(c => c.Name.Length),
            "LengthAsc" => query.OrderBy(c => c.Name.Length),
            _ => query.OrderByDescending(c => c.Blogs.Count),
        };

        var total = query.Count(); // No need for predicate in count now
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var model = new PaginationViewModel<CategoryViewModel>
        {
            CurrentPage = page,
            TotalPages = totalPages,
        };

        if (!this.IsPageValid(page, model.TotalPages))
        {
            this.SetModalMessage("Invalid page", "No categories were found on this page. Page might not exist or was removed.");
            return View(model);
        }

        List<CategoryViewModel> categories;

        if (currentUserMainRole == "User")
        {
            categories = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryViewModel
                {
                    Name = c.Name,
                    CountOfBlogs = c.Blogs.Count(b => b.Visibility && !_context.Bans.Any(bn => bn.UserId == b.AuthorId)),
                })
                .ToList();
        }
        else
        {
            categories = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new CategoryViewModel
                {
                    Name = t.Name,
                    CountOfBlogs = t.Blogs.Count,
                })
                .ToList();
        }

        model.CurrentPage = categories.Count <= 0 ? 0 : page;
        model.Collection = categories;

        return View(model);
    }
}