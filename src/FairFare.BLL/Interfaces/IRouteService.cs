using FairFare.Domain.Entities;

namespace FairFare.BLL.Interfaces;

public interface IRouteService
{
    public RouteLocation AddLocation(TripEvent tripEvent, string locationName, int orderIndex);
    public IReadOnlyList<RouteLocation> GetOrderedLocations(TripEvent tripEvent);

}
