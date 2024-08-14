using System.ComponentModel.DataAnnotations;

namespace TechTales.Models;

public class EditProfileViewModel
{
    [Required]
    [DataType(DataType.Text)]
    public string UserName { get; set; } = null!;
    public string? Country { get; set; }
    public string? AboutMe { get; set; }
    public byte[]? Avatar { get; set; }
}
