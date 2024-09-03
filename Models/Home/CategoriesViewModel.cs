using TechTales.Data.Models;

namespace TechTales.Models.Home;

public class CategoriesViewModel
{
    public List<CategoryViewModel> Categories { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
