namespace TechTales.Data.Models;

public class BlogEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public ICollection<BlogTagEntity> BlogTags { get; set; } = [];
}