using TechTales.Data.Models;

namespace TechTales.Models;

public class BanViewModel
{
    public string? Reason { get; set; }
    public string? EndDate { get; set; }
    public string State { get; set; } = "Allowed";
}