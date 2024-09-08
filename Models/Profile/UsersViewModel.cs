namespace TechTales.Models.Profile;

public class UsersViewModel
{
    public List<UserViewModel> Users { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}