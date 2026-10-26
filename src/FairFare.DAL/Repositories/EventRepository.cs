using FairFare.Domain.Interfaces;
using FairFare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FairFare.DAL.Repositories;

public sealed class EventRepository : IEventRepository
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
        if (_context.Entry(tripEvent).State == EntityState.Detached)
        {
            var known = await LoadKnownIdsAsync(tripEvent.Id);
            var present = new HashSet<Guid>();

            _context.ChangeTracker.TrackGraph(tripEvent, node =>
            {
                if (node.Entry.State != EntityState.Detached) return;
                var id = (Guid)node.Entry.Property("Id").CurrentValue!;
                present.Add(id);
                node.Entry.State = known.Contains(id) ? EntityState.Modified : EntityState.Added;
            });

            await RemoveMissingAsync(known, present);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid eventId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var expenseIds = _context.Expenses.Where(x => x.EventId == eventId).Select(x => x.Id);
        await _context.ExpenseShares.Where(s => expenseIds.Contains(s.ExpenseId)).ExecuteDeleteAsync();
        await _context.Expenses.Where(x => x.EventId == eventId).ExecuteDeleteAsync();
        await _context.Events.Where(e => e.Id == eventId).ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<TripEvent>> FindByTitleAsync(string text) =>
        await _context.Events.AsNoTracking()
            .Where(e => EF.Functions.ILike(e.Title, $"%{text}%"))
            .ToListAsync();

    public async Task<decimal> GetTotalSpentAsync(Guid eventId) =>
        await _context.Expenses
            .Where(x => x.EventId == eventId)
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0m;

    private async Task RemoveMissingAsync(HashSet<Guid> known, HashSet<Guid> present)
    {
        var gone = known.Except(present).ToList();
        if (gone.Count == 0) return;

        _context.ExpenseShares.RemoveRange(await _context.ExpenseShares.Where(s => gone.Contains(s.Id)).ToListAsync());
        _context.Expenses.RemoveRange(await _context.Expenses.Where(x => gone.Contains(x.Id)).ToListAsync());
        _context.RouteLocations.RemoveRange(await _context.RouteLocations.Where(r => gone.Contains(r.Id)).ToListAsync());
        _context.Reports.RemoveRange(await _context.Reports.Where(r => gone.Contains(r.Id)).ToListAsync());
        _context.Participants.RemoveRange(await _context.Participants.Where(p => gone.Contains(p.Id)).ToListAsync());
    }

    private async Task<HashSet<Guid>> LoadKnownIdsAsync(Guid eventId)
    {
        var ids = new HashSet<Guid>();
        ids.UnionWith(await _context.Events.Where(e => e.Id == eventId).Select(e => e.Id).ToListAsync());
        ids.UnionWith(await _context.Participants.Where(p => p.EventId == eventId).Select(p => p.Id).ToListAsync());
        ids.UnionWith(await _context.Expenses.Where(x => x.EventId == eventId).Select(x => x.Id).ToListAsync());
        ids.UnionWith(await _context.ExpenseShares.Where(s => s.Expense!.EventId == eventId).Select(s => s.Id).ToListAsync());
        ids.UnionWith(await _context.RouteLocations.Where(r => r.EventId == eventId).Select(r => r.Id).ToListAsync());
        ids.UnionWith(await _context.Reports.Where(r => r.EventId == eventId).Select(r => r.Id).ToListAsync());
        return ids;
    }
}