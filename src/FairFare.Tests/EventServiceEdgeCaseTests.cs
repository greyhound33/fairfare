using FairFare.BLL.Services;
using FairFare.Domain.Entities;

namespace FairFare.Tests;

public class EventServiceEdgeCaseTests
{
    private static TripEvent CreateTrip(string title = "Trip")
    {
        var service = new EventService();
        return service.CreateEvent(title);
    }

    [Fact]
    public void CreateEvent_uses_default_title_when_blank()
    {
        var service = new EventService();

        var trip = service.CreateEvent("   ");

        Assert.Equal("New trip", trip.Title);
        Assert.True(trip.IsActive);
    }

    [Fact]
    public void CreateEvent_trims_title()
    {
        var service = new EventService();

        var trip = service.CreateEvent("  Carpathians  ");

        Assert.Equal("Carpathians", trip.Title);
    }

    [Fact]
    public void AddParticipant_allows_unique_names_and_rejects_duplicates_ignore_case()
    {
        var service = new EventService();
        var trip = CreateTrip();

        service.AddParticipant(trip, "Anna");

        Assert.Single(trip.Participants);
        Assert.Equal("Anna", trip.Participants[0].Name);

        Assert.Throws<InvalidOperationException>(() => service.AddParticipant(trip, "ANNA"));
    }

    [Fact]
    public void RemoveParticipant_noop_when_id_missing()
    {
        var service = new EventService();
        var trip = CreateTrip();

        service.AddParticipant(trip, "Anna");

        service.RemoveParticipant(trip, Guid.NewGuid());

        Assert.Single(trip.Participants);
    }

    [Fact]
    public void UpdateStatus_can_close_and_reopen_trip()
    {
        var service = new EventService();
        var trip = CreateTrip();

        service.UpdateStatus(trip, false);
        Assert.False(trip.IsActive);

        service.UpdateStatus(trip, true);
        Assert.True(trip.IsActive);
    }

    [Fact]
    public void EventService_methods_throw_when_trip_is_null()
    {
        var service = new EventService();

        Assert.Throws<ArgumentNullException>(() => service.AddParticipant(null!, "Anna"));
        Assert.Throws<ArgumentNullException>(() => service.RemoveParticipant(null!, Guid.NewGuid()));
        Assert.Throws<ArgumentNullException>(() => service.UpdateStatus(null!, true));
    }
}