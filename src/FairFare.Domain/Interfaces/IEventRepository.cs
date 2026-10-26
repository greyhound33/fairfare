using FairFare.Domain.Entities;

namespace FairFare.Domain.Interfaces;

public interface IEventRepository
{
    Task<List<TripEvent>> GetAllAsync();
    Task<TripEvent?> GetByIdAsync(Guid eventId);
    Task SaveAsync(TripEvent tripEvent);
    Task DeleteAsync(Guid eventId);
}