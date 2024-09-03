using System.Diagnostics;
using System.Linq.Expressions;
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

public class FilterController : Controller
{
    private readonly ILogger<ProfileController> _logger;
    private readonly AppDbContext _context;

    public FilterController(ILogger<ProfileController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> FindByCriteria(
        Expression<Func<BlogEntity, bool>> where, 
        string? name, 
        int pageSize = 4, 
        int page = 1)
    {
        name = name?.TrimStart('#');
        ViewBag.Request = name;

        var blogs = await FindByNameAsync(name, where, pageSize, page);
        
        int totalBlogs = blogs.Count;
        int totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

        var model = new FindViewModel
        {
            Blogs = blogs,
            CurrentPage = page,
            TotalPages = totalPages,
            Request = name,
        };

        if (page <= 0 || page > totalPages)
        {
            this.SetModalMessage("Invalid page", "No blogs was found on this page. Page might not exist or was removed.");
            return View("Find", model);
        }

        return View("Find", model);
    }

    public async Task<IActionResult> FindByTags(string? name, int pageSize = 4, int page = 1)
    {
        Expression<Func<BlogEntity, bool>> where = b => b.Tags.Any(t => t.Name.Contains(name ?? string.Empty));
        return await FindByCriteria(where, name, pageSize, page);
    }

    public async Task<IActionResult> FindByCategories(string? name, int pageSize = 4, int page = 1)
    {
        Expression<Func<BlogEntity, bool>> where = b => b.Categories.Any(c => c.Name.Contains(name ?? string.Empty));
        return await FindByCriteria(where, name, pageSize, page);
    }

    [HttpGet]
    public async Task<IActionResult> Find(string? request, string? orderBy, int pageSize = 4, int page = 1)
    {
        ViewBag.Request = request;

        IQueryable<BlogEntity> query = _context.Blogs
            .AsNoTracking()
            .Include(b => b.Author);

        query = orderBy switch
        {
            "dateLate" => query.OrderByDescending(b => b.CreationDate),
            "dateEarly" => query.OrderBy(b => b.CreationDate),
            _ => query.OrderByDescending(b => b.Views),
        };

        if (!string.IsNullOrEmpty(request))
        {
            string trimmedRequest = request.Replace('#', ' ');
            query = query.Where(b =>
                b.Title.Contains(trimmedRequest) || 
                b.Content.Contains(trimmedRequest) || 
                b.Author.UserName!.Contains(trimmedRequest) || 
                b.Tags.Any(t => t.Name.Contains(trimmedRequest)) || 
                b.Categories.Any(c => c.Name.Contains(trimmedRequest))
            );
        }

        var totalBlogs = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

        var model = new FindViewModel
        {
            CurrentPage = page,
            TotalPages = totalPages,
            Request = request,
        };

        if (!this.IsPageValid(page, totalPages))
        {
            this.SetModalMessage("Invalid page", "No blogs was found on this page. Page might not exist or was removed.");
            return View(model);
        }

        // Выборка блогов с учетом пагинации
        var blogs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
    
    private async Task<List<BlogViewModel>> FindByNameAsync(string? name, 
        Expression<Func<BlogEntity, bool>> where, int pageSize = 4, int page = 1)
    {
        return await _context.Blogs
            .AsNoTracking()
            .Where(where)
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
    }
}