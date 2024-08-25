using System.ComponentModel.DataAnnotations;

namespace TechTales.Models;

public class CreateBlogViewModel
{
    public Guid Id { get; set; }
    [Required] [MaxLength(50)] public string Title { get; set; } = null!;
    [Required] public string Content { get; set; } = null!;
    [Required] public bool Visibility { get; set; }
    [Required] public DateTime CreationDate { get; set; }
    public string? Tags { get; set; }
    public string? Categories { get; set; }
}
