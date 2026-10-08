namespace FairFare.Domain.Entities;

public class RouteLocation
{
    public int Id { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
}