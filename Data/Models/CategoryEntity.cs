namespace TechTales.Data.Models;

public class CategoryEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public List<BlogEntity> Blogs { get; set; } = [];
}