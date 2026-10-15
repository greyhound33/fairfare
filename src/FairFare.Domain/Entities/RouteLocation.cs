namespace FairFare.Domain.Entities;

public class RouteLocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string LocationName { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public Guid EventId { get; set; }
    public TripEvent? Event { get; set; }
}