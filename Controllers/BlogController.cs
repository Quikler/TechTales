using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Helpers;
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

    public async Task<IActionResult> Read(Guid id)
    {
        var blog = await _context.Blogs
            .AsNoTracking()
            .Include(b => b.Author)
            .Include(b => b.Comments)
            .ThenInclude(c => c.Author)
            .Include(b => b.Tags)
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == id);
        
        if (blog is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var reader = currentUser is null ?
            // new UserViewModel
            // {
            //     UserName = "Guest",
            //     Avatar = ExtensionMethods.BlobToImageSrc(null),
            // }
            null
            :
            new UserViewModel
            {
                Id = currentUser.Id,
                UserName = currentUser.UserName!,
                Avatar = ExtensionMethods.BlobToImageSrc(currentUser.Avatar),
            };

        ReadBlogViewModel model = new ReadBlogViewModel
        {
            Id = id,
            Title = blog.Title,
            Content = blog.Content,
            Comments = blog.Comments.Select(c => new CommentViewModel
            {
                Content = c.Content,
                Author = new UserViewModel
                {
                    Id = c.AuthorId,
                    UserName = c.Author.UserName!,
                    Avatar = ExtensionMethods.BlobToImageSrc(c.Author.Avatar),
                },
                CreationDate = c.CreationDate,
                IsSameUser = currentUser is not null && currentUser.Id == c.AuthorId,
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
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null || blog.AuthorId != currentUser.Id)
        {
            return Forbid();
        }

        var model = new BlogViewModel
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
    public async Task<IActionResult> Edit(BlogViewModel model)
    {
// Get blog by id
        var blog = await _context.Blogs
            .Include(b => b.Tags)
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == model.Id);

        if (blog is null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null || user.Id != blog.AuthorId)
        {
            return Forbid();
        }

// Create tags to add to db
        var tags = await GetEntitiesAsync(
            model.Tags,
            new char[] { ' ', '#', ',' },
            async s => await _context.Tags.FirstOrDefaultAsync(t => t.Name == s),
            s => new TagEntity { Name = s }
        );
        
// Create categories to add to db
        var categories = await GetEntitiesAsync(
            model.Categories,
            new char[] { ' ', ',' },
            async s => await _context.Categories.FirstOrDefaultAsync(c => c.Name == s),
            s => new CategoryEntity { Name = s }
        );

// Update blog's properties
        blog.Title = model.Title;
        blog.Content = model.Content;
        blog.Visibility = model.Visibility;
        
// Update blog's Tags collection
        blog.Tags.Update(tags ?? []);

// Update blog's Catogories collection
        blog.Categories.Update(categories ?? []);

// Save changes
        await _context.SaveChangesAsync();

        return RedirectToAction("Detail", "Profile", new { Id = user.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var blog = await _context.Blogs
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
        
        if (blog is null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null || blog.AuthorId != currentUser.Id)
        {
            return Forbid();
        }

        _context.Blogs.Remove(blog);
        await _context.SaveChangesAsync();

        return RedirectToAction("Detail", "Profile", new { id = blog.AuthorId });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

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

// Create tags to add to db
        var tags = await GetEntitiesAsync(
            model.Tags,
            new char[] { '#', ' ', ',' },
            async tagName => await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName),
            tagName => new TagEntity { Name = tagName }
        );
        
// Create categories to add to db
        var categories = await GetEntitiesAsync(
            model.Categories, 
            new char[] { ',', ' ' }, 
            async categoryName => await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName), 
            categoryName => new CategoryEntity { Name = categoryName }
        );

        var blogEntity = new BlogEntity
        {
            Title = model.Title,
            Content = model.Content,
            Visibility = model.Visibility,
            Tags = tags ?? [],
            Categories = categories ?? [],
            Author = user,
        };

        await _context.Blogs.AddAsync(blogEntity);
        await _context.SaveChangesAsync();

        return RedirectToAction("Detail", "Profile", new { user.Id });
    }

    private async Task<List<TEntity>> GetEntitiesAsync<TEntity>(
        string? input, 
        char[] separator, 
        Func<string, Task<TEntity?>> entityRetriever, 
        Func<string, TEntity> entityCreator
    ) where TEntity : class
    {
        var entityNames = input?
            .Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var entities = new List<TEntity>();

        if (entityNames is not null)
        {
            foreach (var name in entityNames)
            {
                var entity = await entityRetriever(name) ?? entityCreator(name);
                entities.Add(entity);
            }
        }

        return entities;
    }
}
