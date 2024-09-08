using TechTales.Data.Models;
using TechTales.Models.Blog;

namespace TechTales.Models.Profile;

public class ProfileViewModel
{
    public UserViewModel User { get; set; } = null!;
    public List<BlogViewModel> Blogs { get; set; } = [];
    public bool IsSameUser { get; set; }
    public UserViewModel? CurrentUser { get; set; }
}
