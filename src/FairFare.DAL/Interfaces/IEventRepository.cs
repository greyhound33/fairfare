using FairFare.Domain.Entities;

namespace FairFare.DAL.Interfaces;

public interface ITripEventRepository
{
    Task<List<TripEvent>> GetAllAsync();
    Task<TripEvent?> GetByIdAsync(Guid eventId);
    Task SaveAsync(TripEvent tripEvent);
}