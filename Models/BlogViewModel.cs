using System.ComponentModel.DataAnnotations;

namespace TechTales.Models;

public class BlogViewModel
{
    [Required] public string Title { get; set; } = null!;
    [Required] public string Content { get; set; } = null!;
    [Required] public bool Visibility { get; set; }
    public string? Tags { get; set; }
    public string? Categories { get; set; }
}
