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
using TechTales.Models.Blog;
using TechTales.Models.Comment;

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
// Get blog from db by its Id
        var blogData = await _context.Blogs
            .Where(b => b.Id == id)
            .Select(b => new
            {
                Blog = b,
                Author = b.Author,
                Comments = b.Comments.Select(c => new 
                {
                    c.Id, 
                    c.AuthorId, 
                    c.Content, 
                    c.CreationDate, 
                    Author = new
                    {
                        c.Author.UserName,
                        c.Author.Avatar
                    }
                }),
                Tags = b.Tags.Select(t => t.Name),
                Categories = b.Categories.Select(c => c.Name)
            })
            .FirstOrDefaultAsync();

        if (blogData?.Blog is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var authorized = await _userManager.IsUserAuthorizedAsync(blogData.Blog.AuthorId, currentUser);
        if (!blogData.Blog.Visibility && !authorized)
        {
            return RedirectToAction("Forbidden", "Error");
        }

        var reader = currentUser is null ? null : new UserViewModel
        {
            Id = currentUser.Id,
            UserName = currentUser.UserName!,
            Avatar = currentUser.Avatar.BlobToImageSrc(),
            MainRole = await _userManager.GetMainRoleAsync(currentUser),
        };

        string? userId = currentUser is null
            ? Request.Cookies["VisitorId"] ?? this.GetVisitorId()
            : currentUser.Id.ToString();

        if (userId is not null && !_context.ViewBlogs.Any(v => v.UserId == userId && v.BlogId == blogData.Blog.Id))
        {
            blogData.Blog.Views += 1;

            // Adding a blog view entry to the database
            var view = new ViewBlogEntity
            {
                UserId = userId,
                BlogId = blogData.Blog.Id,
            };

            _context.ViewBlogs.Add(view);
            await _context.SaveChangesAsync();
        }

        ReadBlogViewModel model = new ReadBlogViewModel
        {
            Id = id,
            Title = blogData.Blog.Title,
            Content = blogData.Blog.Content,
            Views = blogData.Blog.Views,
            Comments = blogData.Comments
                .Select(c => new CommentViewModel
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
                    IsSameUser = currentUser is not null && currentUser.Id == c.AuthorId,
                })
                .ToList(),
            Author = new UserViewModel
            {
                Id = blogData.Blog.AuthorId,
                UserName = blogData.Author.UserName!,
                Avatar = ExtensionMethods.BlobToImageSrc(blogData.Author.Avatar),
            },
            Reader = reader,
            Tags = string.Join(' ', blogData.Tags.Select(t => $"#{t}")),
            Categories = string.Join(", ", blogData.Categories.Select(c => c)),
            CreationDate = blogData.Blog.CreationDate,
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

        var authorized = await _userManager.IsUserAuthorizedAsync(blog.AuthorId, User);
        if (!authorized)
        {
            return RedirectToAction("Forbidden", "Error");
        }

        var model = new BlogViewModel
        {
            Id = blog.Id,
            Title = blog.Title,
            Content = blog.Content,
            Visibility = blog.Visibility,
            Tags = string.Join(' ', blog.Tags.Select(t => $"#{t.Name}")),
            Categories = string.Join(", ", blog.Categories.Select(c => c.Name)),
            AuthorId = blog.AuthorId,
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BlogViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var authorized = await _userManager.IsUserAuthorizedAsync(model.AuthorId, currentUser);
        if (!authorized)
        {
            return RedirectToAction("Forbidden", "Error");
        }

        if (!ModelState.IsValid)
        {
            var blogViewModel = await _context.Blogs
                .AsNoTracking()
                .Select(b => new BlogViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    Visibility = b.Visibility,
                    Tags = string.Join(' ', b.Tags.Select(t => "#" + t.Name)),
                    Categories = string.Join(", ", b.Categories.Select(t => "#" + t.Name)),
                })
                .FirstOrDefaultAsync(b => b.Id == model.Id);

            if (blogViewModel is null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            this.ParseModalErrorsAndSet("Validation error");
            return View(blogViewModel);
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

        authorized = await _userManager.IsUserAuthorizedAsync(blog.AuthorId, currentUser);
        if (!authorized)
        {
            return RedirectToAction("Forbidden", "Error");
        }

        var tags = await _context.Tags.ParseAndAddNewAsync(model.Tags ?? string.Empty);
        var categories = await _context.Categories.ParseAndAddNewAsync(model.Categories ?? string.Empty);

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

        return RedirectToAction("Detail", "Profile", new { Id = currentUser?.Id });
    }

    [HttpDelete, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var blog = await _context.Blogs
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (blog is null)
        {
            return RedirectToAction("NotFound", "Error");
        }

        var authorized = await _userManager.IsUserAuthorizedAsync(blog.AuthorId, User);
        if (!authorized)
        {
            return RedirectToAction("Forbidden", "Error");
        }

// Delete unused tags and categories
        await _context.Tags.RemoveUnusedAsync();
        await _context.Categories.RemoveUnusedAsync();

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

        return View(new CreateBlogViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
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

        var tags = await _context.Tags.ParseAndAddNewAsync(model.Tags);
        var categories = await _context.Categories.ParseAndAddNewAsync(model.Categories);

        var blogEntity = new BlogEntity
        {
            Title = model.Title,
            Content = model.Content,
            Visibility = model.Visibility,
            Tags = tags,
            Categories = categories,
            AuthorId = new Guid(id),
        };

        await _context.Blogs.AddAsync(blogEntity);
        await _context.SaveChangesAsync();

        return RedirectToAction("Detail", "Profile", new { id });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? request, string? orderBy, int pageSize = 4, int page = 1)
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

        var currentUser = await _userManager.GetUserAsync(User);
        var currentUserMainRole = await _userManager.GetMainRoleAsync(currentUser);

        query = query.Where(b => currentUserMainRole == "Admin" 
            || currentUserMainRole == "Moderator" ? true : b.Visibility);
        
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

        // Calculate total items and total pages
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var model = new PaginationViewModel<BlogViewModel>
        {
            CurrentPage = page,
            TotalPages = totalPages,
        };

        if (!this.IsPageValid(page, model.TotalPages))
        {
            this.SetModalMessage("Invalid page", "No blogs were found on this page. Page might not exist or was removed.");
            return View(model);
        }

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
                },
                Visibility = b.Visibility,
            })
            .ToListAsync();

        model.Collection = blogs;

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ListByCategories(string? request, string? orderBy, int pageSize = 4, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(request))
        {
            return View("List", new PaginationViewModel<BlogViewModel>());
        }

        var categoryNames = request.Split(new char[] { ' ', '#', ',', '.', '|' }, 
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var query = _context.Categories
            .Include(c => c.Blogs)
                .ThenInclude(b => b.Author)
            .Where(c => categoryNames.Contains(c.Name))
            .SelectMany(c => c.Blogs)
            .AsQueryable();

        // Calculate total items and total pages
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var model = new PaginationViewModel<BlogViewModel>
        {
            CurrentPage = page,
            TotalPages = totalPages,
        };

        if (!this.IsPageValid(page, totalPages))
        {
            this.SetModalMessage("", "");
            return View("List", model);
        }

        // Apply pagination
        var blogs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BlogViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Content = b.Content,
                Views = b.Views,
                CreationDate = b.CreationDate,
                Author = new UserViewModel
                {
                    Id = b.AuthorId,
                    UserName = b.Author.UserName!,
                    Avatar = b.Author.Avatar.BlobToImageSrc("/images/default_user_icon.svg"),
                },
            })
            .ToListAsync();

        model.Collection = blogs;

        return View("List", model);
    }

    [HttpGet]
    public async Task<IActionResult> ListByTags(string? request, string? orderBy, int pageSize = 4, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(request))
        {
            return View("List", new PaginationViewModel<BlogViewModel>());
        }

        var tagNames = request.Split(new char[] { ' ', '#', ',', '.', '|' }, 
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var query = _context.Tags
            .Include(t => t.Blogs)
                .ThenInclude(b => b.Author)
            .Where(t => tagNames.Contains(t.Name))
            .SelectMany(t => t.Blogs)
            .AsQueryable();

        // Calculate total items and total pages
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var model = new PaginationViewModel<BlogViewModel>
        {
            CurrentPage = page,
            TotalPages = totalPages,
        };

        if (!this.IsPageValid(page, totalPages))
        {
            this.SetModalMessage("", "");
            return View("List", model);
        }

        // Apply pagination
        var blogs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BlogViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Content = b.Content,
                Views = b.Views,
                CreationDate = b.CreationDate,
                Author = new UserViewModel
                {
                    Id = b.AuthorId,
                    UserName = b.Author.UserName!,
                    Avatar = b.Author.Avatar.BlobToImageSrc("/images/default_user_icon.svg"),
                },
            })
            .ToListAsync();

        model.Collection = blogs;

        return View("List", model);
    }
}