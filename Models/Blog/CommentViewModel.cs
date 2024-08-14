using System.ComponentModel.DataAnnotations;

namespace TechTales.Models;

public class CommentViewModel
{
    public string Content { get; set; } = null!;
    public UserViewModel Author { get; set; } = null!;
    public bool IsSameUser { get; set; }
    public DateTime CreationDate { get; set; }
}
