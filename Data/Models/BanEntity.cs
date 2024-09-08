namespace TechTales.Data.Models;

public class BanEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid JudgeId { get; set; }
    public DateTime BanStartDate { get; set; } = DateTime.Now;
    public DateTime BanEndDate { get; set; }
    public string BanReason { get; set; } = null!;
}