using System.ComponentModel.DataAnnotations;

namespace TechTales.Models.Blog;

public class BlogViewModel
{
    public Guid Id { get; set; }
    [Required] public string Title { get; set; } = null!;
    [Required] public string Content { get; set; } = null!;
    public UserViewModel? Author { get; set; }
    public Guid AuthorId { get; set; }
    public bool Visibility { get; set; }
    public DateTime CreationDate { get; set; }
    public int Views { get; set; }
    public string? Tags { get; set; }
    public string? Categories { get; set; }
}