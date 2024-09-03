using TechTales.Data.Models;

namespace TechTales.Models;

public class HomeViewModel
{
    public List<BlogViewModel> Blogs { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
