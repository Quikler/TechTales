namespace TechTales.Data.Models;

public class TagEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public List<BlogEntity> Blogs { get; set; } = [];
}