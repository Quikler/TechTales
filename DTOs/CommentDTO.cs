namespace TechTales.DTOs;

public class CommentDTO
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public UserDTO Author { get; set; } = null!;
    public string CreationDate { get; set; } = null!;
}