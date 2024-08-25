namespace TechTales.Models;

public class FindViewModel
{
    public List<BlogViewModel> Blogs { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? Request { get; set; }
}