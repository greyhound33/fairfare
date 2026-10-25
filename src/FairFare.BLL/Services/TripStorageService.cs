using FairFare.Domain.Entities;
using FairFare.Domain.Interfaces;

namespace FairFare.BLL.Services;

public class TripStorageService
{
    private readonly IEventRepository _repository;

    public TripStorageService(IEventRepository repository) => _repository = repository;

    public async Task<TripEvent?> LoadFirstAsync()
    {
        var all = await _repository.GetAllAsync();
        return all.Count > 0 ? all[0] : null;
    }

    public Task SaveAsync(TripEvent tripEvent) => _repository.SaveAsync(tripEvent);
}
