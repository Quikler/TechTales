using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Helpers;
using TechTales.Helpers.Extensions;
using TechTales.Models;

namespace TechTales.Controllers;

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

    [HttpGet]
    public async Task<IActionResult> Read(Guid id)
    {
        var blog = await _context.Blogs
            .Include(b => b.Author)
            .Include(b => b.Comments)
            .ThenInclude(c => c.Author)
            .Include(b => b.Tags)
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == id);
        
        if (blog is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

        var user = await _userManager.GetUserAsync(User);
        var reader = user is null ? null : new UserViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Avatar = user.Avatar.BlobToImageSrc(),
        };

        string? userId = user is null
            ? Request.Cookies["VisitorId"] ?? this.GetVisitorId()
            : user.Id.ToString();

        if (userId is not null && !_context.ViewBlogs.Any(v => v.UserId == userId && v.BlogId == blog.Id))
        {
            blog.Views += 1;

            // Adding a blog view entry to the database
            var view = new ViewBlogEntity
            {
                UserId = userId,
                BlogId = blog.Id,
            };

            _context.ViewBlogs.Add(view);
            await _context.SaveChangesAsync();
        }

        ReadBlogViewModel model = new ReadBlogViewModel
        {
            Id = id,
            Title = blog.Title,
            Content = blog.Content,
            Views = blog.Views,
            Comments = blog.Comments.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Content = c.Content,
                Author = new UserViewModel
                {
                    Id = c.AuthorId,
                    UserName = c.Author.UserName!,
                    Avatar = ExtensionMethods.BlobToImageSrc(c.Author.Avatar),
                },
                CreationDate = c.CreationDate,
                IsSameUser = user is not null && user.Id == c.AuthorId,
            }).ToList(),
            Author = new UserViewModel
            {
                Id = blog.AuthorId,
                UserName = blog.Author.UserName!,
                Avatar = ExtensionMethods.BlobToImageSrc(blog.Author.Avatar),
            },
            Reader = reader,
            Tags = string.Join(' ', blog.Tags.Select(t => $"#{t.Name}")),
            Categories = string.Join(", ", blog.Categories.Select(c => c.Name)),
            CreationDate = blog.CreationDate,
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var blog = await _context.Blogs
            .AsNoTracking()
            .Include(b => b.Tags)
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (blog is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

        var currentUserId = _userManager.GetUserId(User);
        if (blog.AuthorId.ToString() != currentUserId)
        {
            return Forbid();
        }

        var model = new EditBlogViewModel
        {
            Id = blog.Id,
            Title = blog.Title,
            Content = blog.Content,
            Visibility = blog.Visibility,
            Tags = string.Join(' ', blog.Tags.Select(t => $"#{t.Name}")),
            Categories = string.Join(", ", blog.Categories.Select(c => c.Name)),
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditBlogViewModel model)
    {
        if (!ModelState.IsValid)
        {
            this.ParseModalErrorsAndSet("Validation error");
            return View(model);
        }

// Get blog by id
        var blog = await _context.Blogs
            .Include(b => b.Tags)
            .ThenInclude(t => t.Blogs)
            .Include(b => b.Categories)
            .ThenInclude(c => c.Blogs)
            .FirstOrDefaultAsync(b => b.Id == model.Id);

        if (blog is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

        var userId = _userManager.GetUserId(User);
        if (blog.AuthorId.ToString() != userId)
        {
            return Forbid();
        }

        var tags = await _context.Tags.ParseAsync(model.Tags ?? string.Empty);
        var categories = await _context.Categories.ParseAsync(model.Categories ?? string.Empty);

// Update blog's properties
        blog.Title = model.Title;
        blog.Content = model.Content;
        blog.Visibility = model.Visibility;

// Update blog's Tags collection
        blog.Tags = tags;

// Update blog's Catogories collection
        blog.Categories = categories;

// Delete unused tags and categories
        await _context.Tags.RemoveUnusedAsync();
        await _context.Categories.RemoveUnusedAsync();

// Save changes
        await _context.SaveChangesAsync();

        return RedirectToAction("Detail", "Profile", new { Id = userId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var blog = await _context.Blogs
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (blog is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

        var currentUserId = _userManager.GetUserId(User);
        if (blog.AuthorId.ToString() != currentUserId)
        {
            return Forbid();
        }

        _context.Blogs.Attach(blog);
        _context.Blogs.Remove(blog);
        await _context.SaveChangesAsync();

        return RedirectToAction("Detail", "Profile", new { Id = blog.AuthorId });
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (_userManager.GetUserId(User) is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBlogViewModel model)
    {
        var id = _userManager.GetUserId(User);
        if (id is null)
        {
            return RedirectToAction("NotFound", "Error");
        }
        
        if (!ModelState.IsValid)
        {
            this.ParseModalErrorsAndSet("Validation error");
            return View(model);
        }

        var tags = await _context.Tags.ParseAsync(model.Tags);
        var categories = await _context.Categories.ParseAsync(model.Categories);

        var blogEntity = new BlogEntity
        {
            Title = model.Title,
            Content = model.Content,
            Visibility = model.Visibility,
            Tags = tags,
            Categories = categories,
            AuthorId = new Guid(id)
        };
        
        await _context.Blogs.AddAsync(blogEntity);
        await _context.SaveChangesAsync();

        return RedirectToAction("Detail", "Profile", new { id });
    }
}
