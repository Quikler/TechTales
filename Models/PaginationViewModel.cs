namespace TechTales.Models;

public class PaginationViewModel<T> where T : notnull
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public List<T> Collection { get; set; } = [];

    public PaginationViewModel(List<T> collection)
    {
        Collection = collection;
    }
    
    public PaginationViewModel(int current, int totalPages)
    {
        CurrentPage = current;
        TotalPages = totalPages;
    }

    public PaginationViewModel(int totalPages)
    {
        TotalPages = totalPages;
    }

    public PaginationViewModel() { }
}