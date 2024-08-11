using System.ComponentModel.DataAnnotations;

namespace TechTales.Models;

public class LoginViewModel
{
    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "UserNameOrEmail")]
    public string UserNameOrEmail { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = null!;
}
