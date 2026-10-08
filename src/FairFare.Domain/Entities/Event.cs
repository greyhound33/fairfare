namespace FairFare.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<RouteLocation> RouteLocations { get; set; } = new List<RouteLocation>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}