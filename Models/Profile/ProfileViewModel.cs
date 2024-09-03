using TechTales.Data.Models;

namespace TechTales.Models;

public class ProfileViewModel
{
    public UserViewModel User { get; set; } = null!;
    public List<BlogViewModel> Blogs { get; set; } = [];
    public bool IsSameUser { get; set; }
    public UserViewModel? CurrentUser { get; set; }
}
