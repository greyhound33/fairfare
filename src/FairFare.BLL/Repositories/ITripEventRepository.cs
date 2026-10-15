using FairFare.Domain.Entities;

namespace FairFare.BLL.Repositories;

public interface ITripEventRepository
{
    Task<List<TripEvent>> GetAllAsync();
    Task<TripEvent?> GetByIdAsync(Guid eventId);
    Task SaveAsync(TripEvent tripEvent);
}
