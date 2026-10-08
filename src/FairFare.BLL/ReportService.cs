using FairFare.Domain;

namespace FairFare.BLL;

public sealed class ReportService
{
    public Report CreateReport(TripEvent tripEvent, IReadOnlyList<BalanceEntry> balances, IReadOnlyList<Transfer> transfers)
    {
        if (tripEvent is null)
        {
            throw new ArgumentNullException(nameof(tripEvent));
        }

        var summary = new List<string>
        {
            $"Trip report: {tripEvent.Title}",
            $"Participants: {tripEvent.Participants.Count}",
            $"Expenses: {tripEvent.Expenses.Count}",
            $"Transfers: {transfers.Count}"
        };

        foreach (var balance in balances)
        {
            summary.Add($"{balance.ParticipantName}: {balance.Amount:C}");
        }

        if (transfers.Count > 0)
        {
            foreach (var transfer in transfers)
            {
                var fromName = tripEvent.Participants.Single(participant => participant.Id == transfer.FromParticipantId).Name;
                var toName = tripEvent.Participants.Single(participant => participant.Id == transfer.ToParticipantId).Name;
                summary.Add($"{fromName} transfers {transfer.Amount:C} to {toName}.");
            }
        }
        else
        {
            summary.Add("No pending payments are required.");
        }

        var report = new Report
        {
            EventId = tripEvent.Id,
            Event = tripEvent,
            Title = $"{tripEvent.Title} summary",
            Summary = string.Join(Environment.NewLine, summary),
            CreatedAt = DateTime.UtcNow
        };

        tripEvent.Reports.Add(report);
        return report;
    }
}
