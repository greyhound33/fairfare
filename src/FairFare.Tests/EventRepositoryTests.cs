using FairFare.DAL.Repositories;
using FairFare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FairFare.Tests;

public class EventRepositoryTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _db;

    public EventRepositoryTests(PostgresFixture db) => _db = db;

    private static Expense NewExpense(Participant payer, Guid eventId, decimal amount)
    {
        var expense = new Expense { Description = "Item", TotalAmount = amount, PayerId = payer.Id, EventId = eventId };
        expense.Shares.Add(new ExpenseShare { ExpenseId = expense.Id, DebtorId = payer.Id, OwedAmount = amount });
        return expense;
    }

    [Fact]
    public async Task Save_then_GetById_returns_event_with_participants()
    {
        var id = Guid.NewGuid();
        await using (var context = _db.CreateContext())
        {
            var trip = new TripEvent { Id = id, Title = "Test trip" };
            trip.Participants.Add(new Participant { Name = "Anna", EventId = id });
            await new EventRepository(context).SaveAsync(trip);
        }

        await using (var context = _db.CreateContext())
        {
            var loaded = await new EventRepository(context).GetByIdAsync(id);
            Assert.NotNull(loaded);
            Assert.Equal("Test trip", loaded!.Title);
            Assert.Single(loaded.Participants);
        }
    }

    [Fact]
    public async Task Save_existing_event_with_new_participant_adds_participant()
    {
        var id = Guid.NewGuid();
        await using (var context = _db.CreateContext())
        {
            await new EventRepository(context).SaveAsync(new TripEvent { Id = id, Title = "Trip" });
        }

        await using (var context = _db.CreateContext())
        {
            var repository = new EventRepository(context);
            var trip = (await repository.GetByIdAsync(id))!;
            trip.Participants.Add(new Participant { Name = "Maksym", EventId = id });
            await repository.SaveAsync(trip);
        }

        await using (var context = _db.CreateContext())
        {
            Assert.Equal(1, await context.Participants.CountAsync(p => p.EventId == id));
        }
    }

    [Fact]
    public async Task Delete_removes_event_and_all_children()
    {
        var id = Guid.NewGuid();
        await using (var context = _db.CreateContext())
        {
            var trip = new TripEvent { Id = id, Title = "To delete" };
            var anna = new Participant { Name = "Anna", EventId = id };
            var maksym = new Participant { Name = "Maksym", EventId = id };
            trip.Participants.AddRange([anna, maksym]);
            trip.Expenses.Add(NewExpense(anna, id, 100m));
            await new EventRepository(context).SaveAsync(trip);
        }

        await using (var context = _db.CreateContext())
        {
            await new EventRepository(context).DeleteAsync(id);
        }

        await using (var context = _db.CreateContext())
        {
            Assert.False(await context.Events.AnyAsync(e => e.Id == id));
            Assert.Equal(0, await context.Participants.CountAsync(p => p.EventId == id));
            Assert.Equal(0, await context.Expenses.CountAsync(x => x.EventId == id));
            Assert.Equal(0, await context.ExpenseShares.CountAsync(s => s.Expense!.EventId == id));
        }
    }

    [Fact]
    public async Task Save_detached_graph_without_participant_deletes_it()
    {
        var id = Guid.NewGuid();
        var annaId = Guid.NewGuid();

        await using (var context = _db.CreateContext())
        {
            var trip = new TripEvent { Id = id, Title = "Trip" };
            trip.Participants.Add(new Participant { Id = annaId, Name = "Anna", EventId = id });
            trip.Participants.Add(new Participant { Name = "Maksym", EventId = id });
            await new EventRepository(context).SaveAsync(trip);
        }

        await using (var context = _db.CreateContext())
        {
            var trip = new TripEvent { Id = id, Title = "Trip" };
            trip.Participants.Add(new Participant { Id = annaId, Name = "Anna", EventId = id });
            await new EventRepository(context).SaveAsync(trip);
        }

        await using (var context = _db.CreateContext())
        {
            var names = await context.Participants.Where(p => p.EventId == id).Select(p => p.Name).ToListAsync();
            Assert.Equal(new[] { "Anna" }, names);
        }
    }

    [Fact]
    public async Task Duplicate_route_order_in_same_event_is_rejected()
    {
        var id = Guid.NewGuid();
        await using var context = _db.CreateContext();
        var trip = new TripEvent { Id = id, Title = "Route" };
        trip.RouteLocations.Add(new RouteLocation { LocationName = "Lviv", OrderIndex = 1, EventId = id });
        trip.RouteLocations.Add(new RouteLocation { LocationName = "Kyiv", OrderIndex = 1, EventId = id });

        await Assert.ThrowsAsync<DbUpdateException>(() => new EventRepository(context).SaveAsync(trip));
    }

    [Fact]
    public async Task Removing_participant_with_expenses_is_rejected()
    {
        var id = Guid.NewGuid();
        var anna = new Participant { Name = "Anna", EventId = id };
        await using (var context = _db.CreateContext())
        {
            var trip = new TripEvent { Id = id, Title = "Blocked" };
            trip.Participants.Add(anna);
            trip.Expenses.Add(NewExpense(anna, id, 80m));
            await new EventRepository(context).SaveAsync(trip);
        }

        await using (var context = _db.CreateContext())
        {
            var tracked = await context.Participants.SingleAsync(p => p.Id == anna.Id);
            context.Participants.Remove(tracked);
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task GetTotalSpent_sums_expenses_of_event()
    {
        var id = Guid.NewGuid();
        await using var context = _db.CreateContext();
        var anna = new Participant { Name = "Anna", EventId = id };
        var trip = new TripEvent { Id = id, Title = "Spend" };
        trip.Participants.Add(anna);
        trip.Expenses.Add(NewExpense(anna, id, 100m));
        trip.Expenses.Add(NewExpense(anna, id, 50.5m));

        var repository = new EventRepository(context);
        await repository.SaveAsync(trip);

        Assert.Equal(150.5m, await repository.GetTotalSpentAsync(id));
    }

    [Fact]
    public async Task FindByTitle_ignores_case()
    {
        var title = "Carpathians-" + Guid.NewGuid();
        await using var context = _db.CreateContext();
        var repository = new EventRepository(context);
        await repository.SaveAsync(new TripEvent { Id = Guid.NewGuid(), Title = title });

        var found = await repository.FindByTitleAsync(title.ToUpperInvariant());

        Assert.Single(found);
    }
}
