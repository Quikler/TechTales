namespace TechTales.Data.Models;

public class BlogTagEntity
{
    public Guid BlogId { get; set; }
    public BlogEntity? Blog { get; set; }

    public Guid TagId { get; set; }
    public TagEntity? Tag { get; set; }
}