using FairFare.BLL.Services;
using FairFare.Domain.Entities;

namespace FairFare.Tests;

public class RouteServiceTests
{
    [Fact]
    public void AddLocation_validates_blank_name_and_uses_next_order_index()
    {
        var service = new RouteService();
        var trip = new TripEvent { Title = "Trip" };

        var loc1 = service.AddLocation(trip, "  Lviv  ", 0);
        var loc2 = service.AddLocation(trip, "Kyiv", 0);

        Assert.Equal("Lviv", loc1.LocationName);
        Assert.Equal(1, loc1.OrderIndex);
        Assert.Equal("Kyiv", loc2.LocationName);
        Assert.Equal(2, loc2.OrderIndex);

        Assert.Throws<ArgumentException>(() => service.AddLocation(trip, "   ", 0));
    }

    [Fact]
    public void GetOrderedLocations_returns_routes_sorted_by_order_index()
    {
        var service = new RouteService();
        var trip = new TripEvent { Title = "Trip" };

        trip.RouteLocations.Add(new RouteLocation { LocationName = "Kyiv", OrderIndex = 2, EventId = trip.Id });
        trip.RouteLocations.Add(new RouteLocation { LocationName = "Lviv", OrderIndex = 1, EventId = trip.Id });

        var ordered = service.GetOrderedLocations(trip);

        Assert.Equal(new[] { "Lviv", "Kyiv" }, ordered.Select(x => x.LocationName).ToArray());
    }

    [Fact]
    public void AddLocation_throws_for_null_trip()
    {
        var service = new RouteService();

        Assert.Throws<ArgumentNullException>(() => service.AddLocation(null!, "Lviv", 1));
        Assert.Throws<ArgumentNullException>(() => service.GetOrderedLocations(null!));
    }
}