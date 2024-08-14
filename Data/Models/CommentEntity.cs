using Microsoft.AspNetCore.Identity;

namespace TechTales.Data.Models;

public class CommentEntity
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public Guid BlogId { get; set; }
    public BlogEntity Blog { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public UserEntity Author { get; set; } = null!;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime UpdateDate { get; set; } = DateTime.Now;
}