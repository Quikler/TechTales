using Microsoft.AspNetCore.Identity;

namespace TechTales.Data.Models;

public class UserEntity : IdentityUser
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Country { get; set; }
}