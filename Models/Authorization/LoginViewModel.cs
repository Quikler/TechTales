using System.ComponentModel.DataAnnotations;

namespace TechTales.Models;

public class LoginViewModel
{
    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "UserNameOrEmail")]
    [MaxLength(50, ErrorMessage = "Username must be less than 50 symbols")]
    public string UserNameOrEmail { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = null!;
}
