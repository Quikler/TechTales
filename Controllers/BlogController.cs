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
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        var reader = user is null ? null : new UserViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Avatar = ExtensionMethods.BlobToImageSrc(user.Avatar),
        };

        string? userId = user is null
            ? Request.Cookies["VisitorId"] ?? GetVisitorId()
            : user.Id.ToString();

        if (userId is not null && !HasUserViewedBlog(userId, blog.Id))
        {
            blog.Views += 1;
            //_context.Blogs.Update(blog);

            // Добавляем запись о просмотре блога в базу данных
            await AddViewRecordAsync(userId, blog.Id);
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
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null || blog.AuthorId != currentUser.Id)
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
            return View(model);
        }

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
        var tags = await EntityParser.ParseAsync(
            model.Tags,
            new char[] { ' ', '#', ',' },
            async s => await _context.Tags.FirstOrDefaultAsync(t => t.Name == s),
            s => new TagEntity { Name = s }
        );
        
// Create categories to add to db
        var categories = await EntityParser.ParseAsync(
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
    public async Task<IActionResult> Create(CreateBlogViewModel model)
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
        var tags = await EntityParser.ParseAsync(
            model.Tags,
            new char[] { '#', ' ', ',' },
            async tagName => await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName),
            tagName => new TagEntity { Name = tagName }
        );
        
// Create categories to add to db
        var categories = await EntityParser.ParseAsync(
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

        return User.IsInRole("Admin") ? View() : RedirectToAction("Detail", "Profile", new { user.Id });
    }

    private string? GetVisitorId()
    {
        // Проверяем, если кука существует
        if (Request.Cookies.ContainsKey("VisitorId"))
        {
            // Возвращаем существующий идентификатор
            return Request.Cookies["VisitorId"];
        }

        // Если куки нет, создаем новый идентификатор
        var visitorId = Guid.NewGuid().ToString();

        CookieOptions options = new CookieOptions
        {
            Expires = DateTime.Now.AddYears(1),
            HttpOnly = true,
            IsEssential = true,
        };

        // Устанавливаем новую куку
        Response.Cookies.Append("VisitorId", visitorId, options);

        // Возвращаем идентификатор, который только что был создан
        return visitorId;
    }

    private bool HasUserViewedBlog(string userId, Guid blogId)
    {
        return _context.ViewBlogs.Any(v => v.UserId == userId && v.BlogId == blogId);
    }

    // Метод для добавления записи о просмотре блога
    private async Task AddViewRecordAsync(string userId, Guid blogId)
    {
        var view = new ViewBlogEntity
        {
            UserId = userId,
            BlogId = blogId,
        };

        _context.ViewBlogs.Add(view);
        await _context.SaveChangesAsync();
    }
}
