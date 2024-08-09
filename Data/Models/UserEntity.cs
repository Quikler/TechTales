using Microsoft.AspNetCore.Identity;

namespace TechTales.Data.Models;
// maybe add comments to userentity?
public class UserEntity : IdentityUser<Guid>
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Country { get; set; }
    public string? AboutMe { get; set; }
    public byte[]? Avatar { get; set; }
    public List<BlogEntity> Blogs { get; set; } = [];
}