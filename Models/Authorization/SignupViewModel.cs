using System.ComponentModel.DataAnnotations;

namespace TechTales.Models;

public class SignupViewModel
{
    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "UserName")]
    [MaxLength(50, ErrorMessage = "Username must be less than 50 symbols")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    [MaxLength(50, ErrorMessage = "Email must be less than 50 symbols")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "RepeatPassword")]
    [Compare("Password", ErrorMessage = "The password and repeat password do not match.")]
    public string RepeatPassword { get; set; } = string.Empty;
}
