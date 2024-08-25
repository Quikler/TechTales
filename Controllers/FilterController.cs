using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Helpers;
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
    public async Task<IActionResult> Find(string? request, int pageSize = 4, int page = 1)
    {
        var model = await GetModelAsync(request, pageSize, page);

        return View(model);
    }

    private async Task<FindViewModel> GetModelAsync(string? request, int pageSize, int page)
    {
        IQueryable<BlogEntity> query = _context.Blogs
            .AsNoTracking()
            .Include(b => b.Author);

        if (!string.IsNullOrEmpty(request))
        {
            query = query.Where(b =>
                b.Title.Contains(request) || 
                b.Content.Contains(request) || 
                b.Author.UserName!.Contains(request) || 
                b.Tags.Any(t => t.Name.Contains(request.Replace('#', ' '))) || 
                b.Categories.Any(c => c.Name.Contains(request))
            );
        }

        int totalBlogs = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

        List<BlogViewModel> blogs = await query
            .OrderByDescending(b => b.Views)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BlogViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Content = b.Content,
                CreationDate = b.CreationDate,
                Visibility = b.Visibility,
                Tags = string.Join(' ', b.Tags.Select(t => "#" + t.Name)),
                Categories = string.Join(' ', b.Categories.Select(c => c.Name)),
                Views = b.Views,
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

        var model = new FindViewModel
        {
            Blogs = blogs,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return model;
    }
}