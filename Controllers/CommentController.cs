using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechTales.Data;
using TechTales.Data.Models;
using TechTales.Models;

public class CommentController : Controller
{
    private readonly ILogger<BlogController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<UserEntity> _userManager;

    public CommentController(ILogger<BlogController> logger, AppDbContext context,
        UserManager<UserEntity> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid blogId, string content, Guid authorId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null || user.Id != authorId)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return RedirectToAction("Read", "Blog", new { id = blogId });
        }

        var comment = new CommentEntity
        {
            BlogId = blogId,
            Content = content,
            AuthorId = authorId, 
        };

        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
        
        return RedirectToAction("Read", "Blog", new { id = blogId });
    }
}