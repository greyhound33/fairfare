using FairFare.BLL.Repositories;
using FairFare.Domain;
using Microsoft.EntityFrameworkCore;

namespace FairFare.DAL.Repositories;

public sealed class EventRepository : ITripEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TripEvent>> GetAllAsync()
    {
        return await _context.Events
            .Include(e => e.Participants)
            .Include(e => e.Expenses)
                .ThenInclude(e => e.Shares)
            .Include(e => e.RouteLocations)
            .Include(e => e.Reports)
            .ToListAsync();
    }

    public async Task<TripEvent?> GetByIdAsync(Guid eventId)
    {
        return await _context.Events
            .Include(e => e.Participants)
            .Include(e => e.Expenses)
                .ThenInclude(e => e.Shares)
            .Include(e => e.RouteLocations)
            .Include(e => e.Reports)
            .SingleOrDefaultAsync(e => e.Id == eventId);
    }

    public async Task SaveAsync(TripEvent tripEvent)
    {
        if (_context.Events.Any(e => e.Id == tripEvent.Id))
        {
            _context.Events.Update(tripEvent);
        }
        else
        {
            await _context.Events.AddAsync(tripEvent);
        }

        await _context.SaveChangesAsync();
    }
}
