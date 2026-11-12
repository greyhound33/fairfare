using FairFare.BLL.Services;
using FairFare.Domain.Entities;

namespace FairFare.Tests;

public class FinancialServiceTests
{
    private static TripEvent CreateTripWithParticipants(params string[] names)
    {
        var trip = new TripEvent { Title = "Trip" };

        foreach (var name in names)
        {
            trip.Participants.Add(new Participant
            {
                Id = Guid.NewGuid(),
                Name = name,
                EventId = trip.Id
            });
        }

        return trip;
    }

    [Fact]
    public void ValidateExpense_rejects_invalid_payer_and_missing_debtors()
    {
        var service = new FinancialService();
        var trip = CreateTripWithParticipants("Anna", "Maksym");

        var invalidPayer = service.ValidateExpense(trip, Guid.NewGuid(), 100m, new[] { trip.Participants[0].Id });
        Assert.False(invalidPayer.IsValid);
        Assert.Equal("Payer must be an existing participant.", invalidPayer.Message);

        var zeroAmount = service.ValidateExpense(trip, trip.Participants[0].Id, 0m, new[] { trip.Participants[1].Id });
        Assert.False(zeroAmount.IsValid);
        Assert.Equal("Expense total must be greater than zero.", zeroAmount.Message);

        var emptyDebtors = service.ValidateExpense(trip, trip.Participants[0].Id, 100m, Array.Empty<Guid>());
        Assert.False(emptyDebtors.IsValid);
        Assert.Equal("At least one participant must be selected for the split.", emptyDebtors.Message);

        var unknownDebtor = service.ValidateExpense(trip, trip.Participants[0].Id, 100m, new[] { Guid.NewGuid() });
        Assert.False(unknownDebtor.IsValid);
        Assert.Equal("Every debtor must belong to the trip.", unknownDebtor.Message);
    }

    [Fact]
    public void AddExpense_creates_shares_for_all_selected_debtors_and_uses_trimmed_description()
    {
        var service = new FinancialService();
        var trip = CreateTripWithParticipants("Anna", "Maksym", "Oleh");

        var anna = trip.Participants.Single(p => p.Name == "Anna");
        var maksym = trip.Participants.Single(p => p.Name == "Maksym");

        var expense = service.AddExpense(trip, "  Dinner  ", 90m, anna.Id, new[] { anna.Id, maksym.Id });

        Assert.Equal("Dinner", expense.Description);
        Assert.Equal(90m, expense.TotalAmount);
        Assert.Equal(2, expense.Shares.Count);
        Assert.Equal(45m, expense.Shares.Single(s => s.DebtorId == anna.Id).OwedAmount);
        Assert.Equal(45m, expense.Shares.Single(s => s.DebtorId == maksym.Id).OwedAmount);
        Assert.Contains(trip.Expenses, e => e.Id == expense.Id);
    }

    [Fact]
    public void CalculateBalances_returns_each_participant_net_amount()
    {
        var service = new FinancialService();
        var trip = CreateTripWithParticipants("Anna", "Maksym");

        var anna = trip.Participants.Single(p => p.Name == "Anna");
        var maksym = trip.Participants.Single(p => p.Name == "Maksym");

        var expense1 = new Expense { TotalAmount = 100m, PayerId = anna.Id, EventId = trip.Id };
        expense1.Shares.Add(new ExpenseShare { DebtorId = anna.Id, OwedAmount = 50m });
        expense1.Shares.Add(new ExpenseShare { DebtorId = maksym.Id, OwedAmount = 50m });

        var expense2 = new Expense { TotalAmount = 40m, PayerId = maksym.Id, EventId = trip.Id };
        expense2.Shares.Add(new ExpenseShare { DebtorId = anna.Id, OwedAmount = 20m });
        expense2.Shares.Add(new ExpenseShare { DebtorId = maksym.Id, OwedAmount = 20m });

        trip.Expenses.Add(expense1);
        trip.Expenses.Add(expense2);

        var balances = service.CalculateBalances(trip);

        Assert.Equal(2, balances.Count);
        Assert.Equal(30m, balances.Single(b => b.ParticipantName == "Anna").Amount);
        Assert.Equal(-30m, balances.Single(b => b.ParticipantName == "Maksym").Amount);
    }

    [Fact]
    public void OptimizeTransfers_produces_minimal_set_of_transfers()
    {
        var service = new FinancialService();
        var trip = CreateTripWithParticipants("Anna", "Maksym", "Oleh");

        var anna = trip.Participants.Single(p => p.Name == "Anna");
        var maksym = trip.Participants.Single(p => p.Name == "Maksym");
        var oleh = trip.Participants.Single(p => p.Name == "Oleh");

        var expense = new Expense { TotalAmount = 300m, PayerId = anna.Id, EventId = trip.Id };
        expense.Shares.Add(new ExpenseShare { DebtorId = anna.Id, OwedAmount = 100m });
        expense.Shares.Add(new ExpenseShare { DebtorId = maksym.Id, OwedAmount = 100m });
        expense.Shares.Add(new ExpenseShare { DebtorId = oleh.Id, OwedAmount = 100m });

        trip.Expenses.Add(expense);

        var transfers = service.OptimizeTransfers(trip);

        Assert.Equal(2, transfers.Count);

        Assert.All(transfers, transfer =>
        {
            Assert.Equal(anna.Id, transfer.ToParticipantId);
            Assert.Equal(100m, transfer.Amount);
        });

        Assert.Contains(transfers, transfer => transfer.FromParticipantId == maksym.Id);
        Assert.Contains(transfers, transfer => transfer.FromParticipantId == oleh.Id);
        Assert.Equal(200m, transfers.Sum(transfer => transfer.Amount));
    }
}