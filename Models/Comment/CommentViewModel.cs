using System.ComponentModel.DataAnnotations;
using TechTales.Models.Blog;

namespace TechTales.Models.Comment;

public class CommentViewModel
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public UserViewModel Author { get; set; } = null!;
    public bool IsSameUser { get; set; }
    public DateTime CreationDate { get; set; }
    public BlogViewModel? Blog { get; set; }
}
