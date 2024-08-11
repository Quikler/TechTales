namespace TechTales.Models;

public class ProfileViewModel
{
    public string? UserName { get; set; }
    public string? Country { get; set; }
    public string? AboutMe { get; set; }
    public byte[]? Avatar { get; set; }
    public bool IsSameUser { get; set; }
}
