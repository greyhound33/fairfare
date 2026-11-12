using FairFare.BLL.Services;
using FairFare.Domain.Entities;

namespace FairFare.Tests;

public class EventServiceParticipantTests
{
    private static (EventService Service, TripEvent Trip) CreateTrip()
    {
        var service = new EventService();
        return (service, service.CreateEvent("Trip"));
    }

    private static void AddExpense(TripEvent trip, Participant payer, Participant debtor)
    {
        var expense = new Expense { Description = "Item", TotalAmount = 10m, PayerId = payer.Id, EventId = trip.Id };
        expense.Shares.Add(new ExpenseShare { ExpenseId = expense.Id, DebtorId = debtor.Id, OwedAmount = 10m });
        trip.Expenses.Add(expense);
    }

    [Fact]
    public void RemoveParticipant_removes_participant_without_expenses()
    {
        var (service, trip) = CreateTrip();
        service.AddParticipant(trip, "Anna");
        service.AddParticipant(trip, "Maksym");

        service.RemoveParticipant(trip, trip.Participants.Single(p => p.Name == "Maksym").Id);

        Assert.Equal(new[] { "Anna" }, trip.Participants.Select(p => p.Name).ToArray());
    }

    [Fact]
    public void RemoveParticipant_is_blocked_for_payer()
    {
        var (service, trip) = CreateTrip();
        service.AddParticipant(trip, "Anna");
        service.AddParticipant(trip, "Maksym");
        var anna = trip.Participants.Single(p => p.Name == "Anna");
        var maksym = trip.Participants.Single(p => p.Name == "Maksym");
        AddExpense(trip, anna, maksym);

        Assert.Throws<InvalidOperationException>(() => service.RemoveParticipant(trip, anna.Id));
        Assert.Equal(2, trip.Participants.Count);
    }

    [Fact]
    public void RemoveParticipant_is_blocked_for_debtor()
    {
        var (service, trip) = CreateTrip();
        service.AddParticipant(trip, "Anna");
        service.AddParticipant(trip, "Maksym");
        var anna = trip.Participants.Single(p => p.Name == "Anna");
        var maksym = trip.Participants.Single(p => p.Name == "Maksym");
        AddExpense(trip, anna, maksym);

        Assert.Throws<InvalidOperationException>(() => service.RemoveParticipant(trip, maksym.Id));
        Assert.Equal(2, trip.Participants.Count);
    }

    [Fact]
    public void AddParticipant_rejects_duplicate_ignoring_case()
    {
        var (service, trip) = CreateTrip();
        service.AddParticipant(trip, "Anna");

        Assert.Throws<InvalidOperationException>(() => service.AddParticipant(trip, "ANNA"));
    }

    [Fact]
    public void AddParticipant_rejects_blank_name()
    {
        var (service, trip) = CreateTrip();

        Assert.Throws<ArgumentException>(() => service.AddParticipant(trip, "   "));
    }

    [Fact]
    public void AddParticipant_is_blocked_for_completed_trip()
    {
        var (service, trip) = CreateTrip();
        service.AddParticipant(trip, "Anna");
        service.UpdateStatus(trip, false);

        Assert.Throws<InvalidOperationException>(() => service.AddParticipant(trip, "Maksym"));
        Assert.Single(trip.Participants);
    }

    [Fact]
    public void RemoveParticipant_is_blocked_for_completed_trip()
    {
        var (service, trip) = CreateTrip();
        service.AddParticipant(trip, "Anna");
        service.AddParticipant(trip, "Maksym");
        service.UpdateStatus(trip, false);

        Assert.Throws<InvalidOperationException>(() => service.RemoveParticipant(trip, trip.Participants[1].Id));
        Assert.Equal(2, trip.Participants.Count);
    }
}
