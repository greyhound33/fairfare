using FairFare.Domain;

namespace FairFare.BLL;

public sealed class EventService
{
    public TripEvent CreateEvent(string title)
    {
        var tripEvent = new TripEvent
        {
            Title = string.IsNullOrWhiteSpace(title) ? "New trip" : title.Trim(),
            IsActive = true
        };

        return tripEvent;
    }

    public void AddParticipant(TripEvent tripEvent, string participantName)
    {
        if (tripEvent is null)
        {
            throw new ArgumentNullException(nameof(tripEvent));
        }

        var name = participantName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Participant name is required.", nameof(participantName));
        }

        if (tripEvent.Participants.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Participant already exists in this trip.");
        }

        tripEvent.Participants.Add(new Participant
        {
            Name = name,
            EventId = tripEvent.Id,
            Event = tripEvent
        });
    }

    public void RemoveParticipant(TripEvent tripEvent, Guid participantId)
    {
        if (tripEvent is null)
        {
            throw new ArgumentNullException(nameof(tripEvent));
        }

        var participant = tripEvent.Participants.SingleOrDefault(p => p.Id == participantId);
        if (participant is null)
        {
            return;
        }

        if (tripEvent.Expenses.Any(expense => expense.PayerId == participantId || expense.Shares.Any(share => share.DebtorId == participantId)))
        {
            throw new InvalidOperationException("Participant is still linked to an expense and cannot be removed.");
        }

        tripEvent.Participants.Remove(participant);
    }

    public void UpdateStatus(TripEvent tripEvent, bool isActive)
    {
        if (tripEvent is null)
        {
            throw new ArgumentNullException(nameof(tripEvent));
        }

        tripEvent.IsActive = isActive;
    }
}
