using FairFare.BLL.Services;
using FairFare.Domain.Entities;
using FairFare.Domain.Models;

namespace FairFare.Tests;

public class ReportServiceTests
{
    private static TripEvent CreateTripWithParticipants(params string[] names)
    {
        var trip = new TripEvent { Title = "Weekend" };

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
    public void CreateReport_with_no_pending_transfers_adds_summary_message()
    {
        var trip = CreateTripWithParticipants("Anna", "Maksym");
        var service = new ReportService();

        var balances = new[]
        {
            new BalanceEntry { ParticipantId = trip.Participants[0].Id, ParticipantName = "Anna", Amount = 0m },
            new BalanceEntry { ParticipantId = trip.Participants[1].Id, ParticipantName = "Maksym", Amount = 0m }
        };

        var report = service.CreateReport(trip, balances, Array.Empty<Transfer>());

        Assert.Equal("Weekend summary", report.Title);
        Assert.Contains("Trip report: Weekend", report.Summary);
        Assert.Contains("Participants: 2", report.Summary);
        Assert.Contains("No pending payments are required.", report.Summary);
        Assert.Same(trip, report.Event);
        Assert.Contains(report, trip.Reports);
    }

    [Fact]
    public void CreateReport_with_transfers_adds_payment_lines_for_each_transfer()
    {
        var trip = CreateTripWithParticipants("Anna", "Maksym");
        var service = new ReportService();

        var anna = trip.Participants.Single(p => p.Name == "Anna");
        var maksym = trip.Participants.Single(p => p.Name == "Maksym");

        var balances = new[]
        {
            new BalanceEntry { ParticipantId = anna.Id, ParticipantName = "Anna", Amount = 50m },
            new BalanceEntry { ParticipantId = maksym.Id, ParticipantName = "Maksym", Amount = -50m }
        };

        var transfers = new[]
        {
            new Transfer { FromParticipantId = maksym.Id, ToParticipantId = anna.Id, Amount = 50m }
        };

        var report = service.CreateReport(trip, balances, transfers);

        Assert.Contains("Maksym transfers", report.Summary);
        Assert.Contains("to Anna", report.Summary);
    }

    [Fact]
    public void CreateReport_throws_for_null_trip()
    {
        var service = new ReportService();

        Assert.Throws<ArgumentNullException>(() =>
            service.CreateReport(null!, new[] { new BalanceEntry { ParticipantName = "Anna", Amount = 0m } }, Array.Empty<Transfer>()));
    }
}