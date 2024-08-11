namespace TechTales.Data.Models;

public class BlogEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public UserEntity Author { get; set; } = null!;
    public bool Visibility { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public int Views { get; set; }
    public List<CommentEntity> Comments { get; set; } = [];
    public List<TagEntity> Tags { get; set; } = [];
    public List<CategoryEntity> Catogories { get; set; } = [];
}