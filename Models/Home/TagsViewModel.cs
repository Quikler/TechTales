using TechTales.Data.Models;

namespace TechTales.Models.Home;

public class TagsViewModel
{
    public List<TagViewModel> Tags { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
