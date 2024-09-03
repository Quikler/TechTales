using System.ComponentModel.DataAnnotations;
using TechTales.Data.Models;

namespace TechTales.Models.Home;

public class ContactsViewModel
{
    [Required] public string Name { get; set; } = null!;
    [EmailAddress, Required] public string Email { get; set; } = null!;
    [Required] public string Message { get; set; } = null!;
}