using FairFare.Domain;

namespace FairFare.BLL;

public sealed class RouteService
{
    public RouteLocation AddLocation(TripEvent tripEvent, string locationName, int orderIndex)
    {
        if (tripEvent is null)
        {
            throw new ArgumentNullException(nameof(tripEvent));
        }

        var name = locationName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Location name is required.", nameof(locationName));
        }

        var nextOrder = orderIndex > 0 ? orderIndex : tripEvent.RouteLocations.Count + 1;
        var routeLocation = new RouteLocation
        {
            LocationName = name,
            OrderIndex = nextOrder,
            EventId = tripEvent.Id,
            Event = tripEvent
        };

        tripEvent.RouteLocations.Add(routeLocation);
        tripEvent.RouteLocations = tripEvent.RouteLocations
            .OrderBy(location => location.OrderIndex)
            .ToList();

        return routeLocation;
    }

    public IReadOnlyList<RouteLocation> GetOrderedLocations(TripEvent tripEvent)
    {
        if (tripEvent is null)
        {
            throw new ArgumentNullException(nameof(tripEvent));
        }

        return tripEvent.RouteLocations
            .OrderBy(location => location.OrderIndex)
            .ToList();
    }
}
