namespace FairFare.Domain.Entities;

public class TripEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<Participant> Participants { get; set; } = new();
    public List<Expense> Expenses { get; set; } = new();
    public List<RouteLocation> RouteLocations { get; set; } = new();
    public List<Report> Reports { get; set; } = new();
}