using System.ComponentModel.DataAnnotations;

namespace TechTales.Models.Profile;

public class EditProfileViewModel
{
    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "UserName")]
    [MaxLength(50, ErrorMessage = "Username must be less than 50 symbols")]
    public string UserName { get; set; } = null!;
    public string? Country { get; set; }
    public string? AboutMe { get; set; }
    public string? Avatar { get; set; }
}
