using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.DTOs;
using TechTales.Helpers;
using TechTales.Helpers.Extensions;
using TechTales.Hubs;
using TechTales.Models;
using TechTales.Models.Blog;
using TechTales.Models.Comment;

namespace TechTales.Controllers;

public class CommentController : Controller
{
    private readonly ILogger<BlogController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IHubContext<CommentHub> _commentHub;

    public CommentController(ILogger<BlogController> logger, AppDbContext context,
        UserManager<UserEntity> userManager, IHubContext<CommentHub> commentHub)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _commentHub = commentHub;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid blogId, string content, Guid readerId)
    {
        var user = await _userManager.GetUserAsync(User);
        var authorized = await _userManager.IsUserAuthorizedAsync(readerId, user);

        if (user is null || user.Id != readerId || !authorized)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(new { Title = "Validation error", Text = "Comment content cannot be empty." });
        }

        var comment = new CommentEntity
        {
            Id = Guid.NewGuid(),
            BlogId = blogId,
            Content = content,
            AuthorId = readerId, 
        };

        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
        
        var commentDTO = new CommentDTO
        {
            Id = comment.Id,
            Content = comment.Content,
            CreationDate = comment.CreationDate.ToString("dd/MM/yyyy HH:mm:ss"),
            Author = new UserDTO 
            {
                Id = comment.AuthorId, 
                UserName = user.UserName!, 
                Avatar = user.Avatar.BlobToImageSrc(), 
            }
        };

        return Ok(commentDTO);
    }

    [HttpDelete, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment is null || comment.Id == Guid.Empty)
        {
            return BadRequest("No such comment in database.");
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null || currentUser.Id != comment.AuthorId)
        {
            return Forbid("Invalid user.");
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        
        return Ok($"Comment '{id}' has been deleted.");
    }

    [HttpPatch, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, string content)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment is null || comment.Id == Guid.Empty)
        {
            return BadRequest("No such comment in database.");
        }

        var authorized = await _userManager.IsUserAuthorizedAsync(comment.AuthorId, User);
        if (!authorized)
        {
            return Forbid();
        }

        comment.Content = content.Replace("<br>", "\n");
        await _context.SaveChangesAsync();

        return Ok(comment.Content);
    }

    [HttpGet]
    public async Task<IActionResult> List(string? request, string? orderBy, int pageSize = 5, int page = 1)
    {
        var total = await _context.Comments.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var model = new PaginationViewModel<CommentViewModel>
        {
            CurrentPage = page,
            TotalPages = totalPages
        };

        if (page <= 0 || page > totalPages)
        {
            this.SetModalMessage("Invalid page", "No comments were found on this page. Page might not exist or was removed.");
            return View(model);
        }

        var comments = await _context.Comments
            .AsNoTracking()
            .Where(c => c.Content.Contains(string.Empty))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Author)
            .Include(c => c.Blog)
            .Select(c => new CommentViewModel
            {
                Id = c.Id,
                Content = c.Content,
                CreationDate = c.CreationDate,
                Author = new UserViewModel
                {
                    Id = c.AuthorId,
                    UserName = c.Author.UserName!,
                    Avatar = c.Author.Avatar.BlobToImageSrc("/images/default_user_icon.svg"),
                },
                Blog = new BlogViewModel
                {
                    Id = c.BlogId,
                },
            })
            .ToListAsync();

        model.Collection = comments;

        return View(model);
    }
}