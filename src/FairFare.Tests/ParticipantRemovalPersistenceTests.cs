using FairFare.DAL.Repositories;
using FairFare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FairFare.Tests;

// Відтворює шлях UI: подію завантажено в той самий DbContext, з якого її зберігають.
public class ParticipantRemovalPersistenceTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _db;

    public ParticipantRemovalPersistenceTests(PostgresFixture db) => _db = db;

    [Fact]
    public async Task Removing_participant_from_loaded_event_deletes_it_from_db()
    {
        var id = Guid.NewGuid();

        await using (var context = _db.CreateContext())
        {
            var trip = new TripEvent { Id = id, Title = "Tracked" };
            trip.Participants.Add(new Participant { Name = "Anna", EventId = id });
            trip.Participants.Add(new Participant { Name = "Maksym", EventId = id });
            await new EventRepository(context).SaveAsync(trip);
        }

        await using (var context = _db.CreateContext())
        {
            var repository = new EventRepository(context);
            var loaded = (await repository.GetByIdAsync(id))!;
            loaded.Participants.Remove(loaded.Participants.Single(p => p.Name == "Maksym"));
            await repository.SaveAsync(loaded);
        }

        await using var check = _db.CreateContext();
        var names = await check.Participants.Where(p => p.EventId == id).Select(p => p.Name).ToListAsync();
        Assert.Equal(new[] { "Anna" }, names);
    }
}
