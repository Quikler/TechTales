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

    public CategoryController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public IActionResult List(string? request, string? orderBy, int pageSize = 10, int page = 1)
    {
        IQueryable<CategoryEntity> query = _context.Categories
            .AsNoTracking()
            .Include(c => c.Blogs)
            .Where(c => c.Blogs.Count > 0);

        if (request is not null)
        {
            var entityNames = request.Split(new char[] { ' ', '#', ',', '.', '|' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            query = _context.Categories
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

        var total = query.Count();
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

        var categories = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryViewModel
            {
                Name = c.Name,
                CountOfBlogs = c.Blogs.Count,
            })
            .ToList();

        model.CurrentPage = categories.Count <= 0 ? 0 : page;

        model.Collection = categories;

        return View(model);
    }
}