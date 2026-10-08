using FairFare.BLL;
using FairFare.Domain;

namespace FairFare.Tests;

public class FinancialModuleTests
{
    [Fact]
    public void AddExpense_ShouldCalculateBalancesCorrectly()
    {
        var trip = new TripEvent { Title = "Weekend test" };
        var anna = new Participant { Id = Guid.NewGuid(), Name = "Anna" };
        var maks = new Participant { Id = Guid.NewGuid(), Name = "Maks" };
        var lena = new Participant { Id = Guid.NewGuid(), Name = "Lena" };

        trip.Participants.AddRange(new[] { anna, maks, lena });

        var module = new FinancialModule();
        module.AddExpense(trip, "Hotel", 300m, anna.Id, new[] { anna.Id, maks.Id, lena.Id });

        var balances = module.CalculateBalances(trip);

        Assert.Equal(200m, balances.Single(entry => entry.ParticipantName == "Anna").Amount);
        Assert.Equal(-100m, balances.Single(entry => entry.ParticipantName == "Maks").Amount);
        Assert.Equal(-100m, balances.Single(entry => entry.ParticipantName == "Lena").Amount);
    }

    [Fact]
    public void OptimizeTransfers_ShouldReturnSingleTransferForTwoParticipants()
    {
        var trip = new TripEvent { Title = "Two-person trip" };
        var anna = new Participant { Id = Guid.NewGuid(), Name = "Anna" };
        var maks = new Participant { Id = Guid.NewGuid(), Name = "Maks" };

        trip.Participants.AddRange(new[] { anna, maks });

        var module = new FinancialModule();
        module.AddExpense(trip, "Dinner", 100m, anna.Id, new[] { anna.Id, maks.Id });

        var transfers = module.OptimizeTransfers(trip);

        var transfer = Assert.Single(transfers);
        Assert.Equal(maks.Id, transfer.FromParticipantId);
        Assert.Equal(anna.Id, transfer.ToParticipantId);
        Assert.Equal(50m, transfer.Amount);
    }

    [Fact]
    public void ValidateExpense_ShouldRejectZeroAmount()
    {
        var trip = new TripEvent { Title = "Zero amount test" };
        var anna = new Participant { Id = Guid.NewGuid(), Name = "Anna" };
        var maks = new Participant { Id = Guid.NewGuid(), Name = "Maks" };

        trip.Participants.AddRange(new[] { anna, maks });

        var module = new FinancialModule();
        var result = module.ValidateExpense(trip, anna.Id, 0m, new[] { maks.Id });

        Assert.False(result.IsValid);
        Assert.Contains("greater than zero", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}

