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

    [HttpPost]
    public async Task<IActionResult> Add(Guid blogId, string content, Guid readerId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null || user.Id != readerId)
        {
            return Forbid("Invalid user.");
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

    [HttpDelete]
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

    [HttpPut]
    public async Task<IActionResult> Edit(Guid id, string content)
    {
        var comment = await _context.Comments
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

        comment.Content = content.Replace("<br>", "\n");
        await _context.SaveChangesAsync();
        
        return Ok(comment.Content);
    }
}