namespace FairFare.Domain.Entities;

public class Report
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int EventId { get; set; }
    public Event? Event { get; set; }
}