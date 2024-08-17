namespace TechTales.DTOs;

public class UserDTO
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Avatar { get; set; } = null!;
}