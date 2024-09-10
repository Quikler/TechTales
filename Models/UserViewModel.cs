using TechTales.Data.Models;

namespace TechTales.Models;

public class UserViewModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Country { get; set; }
    public string? AboutMe { get; set; }
    public string? Avatar { get; set; }
    public string MainRole { get; set; } = "User";
    public int CountOfBlogs { get; set; }
    public BanViewModel? Ban { get; set; }
}