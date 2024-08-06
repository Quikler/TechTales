namespace TechTales.Data.Models;

public class TagEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<BlogTagEntity> BlogTags { get; set; } = [];
}