using FairFare.DAL.Repositories;
using FairFare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FairFare.Tests;

public class EventRepositoryEdgeCaseTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _db;

    public EventRepositoryEdgeCaseTests(PostgresFixture db) => _db = db;

    [Fact]
    public async Task GetTotalSpent_is_zero_when_event_has_no_expenses()
    {
        await using var context = _db.CreateContext();
        var repository = new EventRepository(context);
        var id = Guid.NewGuid();
        await repository.SaveAsync(new TripEvent { Id = id, Title = "Empty" });

        Assert.Equal(0m, await repository.GetTotalSpentAsync(id));
    }

    [Fact]
    public async Task FindByTitle_returns_empty_when_nothing_matches()
    {
        await using var context = _db.CreateContext();
        var found = await new EventRepository(context).FindByTitleAsync("no-such-" + Guid.NewGuid());

        Assert.Empty(found);
    }

    [Fact]
    public async Task Delete_of_missing_event_does_nothing()
    {
        await using var context = _db.CreateContext();
        var exception = await Record.ExceptionAsync(() => new EventRepository(context).DeleteAsync(Guid.NewGuid()));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Saving_same_graph_twice_does_not_duplicate_children()
    {
        var id = Guid.NewGuid();
        var annaId = Guid.NewGuid();

        for (var i = 0; i < 2; i++)
        {
            await using var context = _db.CreateContext();
            var trip = new TripEvent { Id = id, Title = "Twice" };
            trip.Participants.Add(new Participant { Id = annaId, Name = "Anna", EventId = id });
            await new EventRepository(context).SaveAsync(trip);
        }

        await using var check = _db.CreateContext();
        Assert.Equal(1, await check.Participants.CountAsync(p => p.EventId == id));
    }
}
