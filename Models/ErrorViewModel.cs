using TechTales.Data.Models;

namespace TechTales.Models;

public class ErrorViewModel
{
    public int Code { get; set; }
    public string? Header { get; set; }
    public string? Reason { get; set; }
    public string? BanEndDate { get; set; }
}