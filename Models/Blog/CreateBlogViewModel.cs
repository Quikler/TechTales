using System.ComponentModel.DataAnnotations;

namespace TechTales.Models.Blog;

public class CreateBlogViewModel
{
    public Guid Id { get; set; }
    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Title")]
    [MaxLength(50, ErrorMessage = "Title must be less than 50 symbols")]
    public string Title { get; set; } = null!;
    [Required] public string Content { get; set; } = null!;
    [Required] public bool Visibility { get; set; }
    [Required] public DateTime CreationDate { get; set; }
    public string? Tags { get; set; }
    public string? Categories { get; set; }
}
