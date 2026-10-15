using FairFare.Domain.Entities;

namespace FairFare.BLL.Interfaces;

public interface IEventService
{
    public TripEvent CreateEvent(string title);
    public void AddParticipant(TripEvent tripEvent, string participantName);
    public void RemoveParticipant(TripEvent tripEvent, Guid participantId);
    public void UpdateStatus(TripEvent tripEvent, bool isActive);
}
