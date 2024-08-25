namespace TechTales.Data.Models;

public class ViewBlogEntity
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public Guid BlogId { get; set; }
}