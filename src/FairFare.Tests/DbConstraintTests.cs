using FairFare.DAL.Repositories;
using FairFare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FairFare.Tests;

public class DbConstraintTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _db;

    public DbConstraintTests(PostgresFixture db) => _db = db;

    private static TripEvent TripWithExpense(string title, decimal total, decimal share)
    {
        var id = Guid.NewGuid();
        var anna = new Participant { Name = "Anna", EventId = id };
        var trip = new TripEvent { Id = id, Title = title };
        trip.Participants.Add(anna);
        var expense = new Expense { Description = "Item", TotalAmount = total, PayerId = anna.Id, EventId = id };
        expense.Shares.Add(new ExpenseShare { ExpenseId = expense.Id, DebtorId = anna.Id, OwedAmount = share });
        trip.Expenses.Add(expense);
        return trip;
    }

    [Fact]
    public async Task Valid_expense_is_saved()
    {
        await using var context = _db.CreateContext();
        var trip = TripWithExpense("Valid", 10m, 10m);

        await new EventRepository(context).SaveAsync(trip);

        Assert.Equal(1, await context.Expenses.CountAsync(e => e.EventId == trip.Id));
    }

    [Fact]
    public async Task Blank_trip_title_is_rejected()
    {
        await using var context = _db.CreateContext();
        var trip = new TripEvent { Id = Guid.NewGuid(), Title = "   " };

        await Assert.ThrowsAsync<DbUpdateException>(() => new EventRepository(context).SaveAsync(trip));
    }

    [Fact]
    public async Task Duplicate_participant_name_ignoring_case_is_rejected()
    {
        await using var context = _db.CreateContext();
        var id = Guid.NewGuid();
        var trip = new TripEvent { Id = id, Title = "Dup" };
        trip.Participants.Add(new Participant { Name = "Anna", EventId = id });
        trip.Participants.Add(new Participant { Name = "ANNA", EventId = id });

        await Assert.ThrowsAsync<DbUpdateException>(() => new EventRepository(context).SaveAsync(trip));
    }

    [Fact]
    public async Task Same_participant_name_in_different_events_is_allowed()
    {
        await using var context = _db.CreateContext();
        var repository = new EventRepository(context);
        var eventIds = new List<Guid>();

        for (var i = 0; i < 2; i++)
        {
            var id = Guid.NewGuid();
            eventIds.Add(id);
            var trip = new TripEvent { Id = id, Title = "Event " + id };
            trip.Participants.Add(new Participant { Name = "Anna", EventId = id });
            await repository.SaveAsync(trip);
        }

        Assert.Equal(2, await context.Participants.CountAsync(p => p.Name == "Anna" && eventIds.Contains(p.EventId)));
    }

    [Fact]
    public async Task Non_positive_expense_amount_is_rejected()
    {
        await using var context = _db.CreateContext();
        var trip = TripWithExpense("Zero", 0m, 0m);

        await Assert.ThrowsAsync<DbUpdateException>(() => new EventRepository(context).SaveAsync(trip));
    }

    [Fact]
    public async Task Negative_share_is_rejected()
    {
        await using var context = _db.CreateContext();
        var trip = TripWithExpense("Negative", 10m, -5m);

        await Assert.ThrowsAsync<DbUpdateException>(() => new EventRepository(context).SaveAsync(trip));
    }
}
