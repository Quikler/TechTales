using System.ComponentModel.DataAnnotations;

namespace TechTales.Models;

public class ReadBlogViewModel
{
    public Guid Id { get; set; }
    [Required] public string Title { get; set; } = null!;
    [Required] public string Content { get; set; } = null!;
    [Required] public DateTime CreationDate { get; set; }
    [Required] public UserViewModel Author { get; set; } = null!;
    [Required] public UserViewModel? Reader { get; set; }
    public string? Tags { get; set; }
    public string? Categories { get; set; }
    public List<CommentViewModel> Comments { get; set; } = [];
}
