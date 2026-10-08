using FairFare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FairFare.DAL.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseShare> ExpenseShares => Set<ExpenseShare>();
    public DbSet<RouteLocation> RouteLocations => Set<RouteLocation>();
    public DbSet<Report> Reports => Set<Report>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>()
            .Property(e => e.TotalAmount)
            .HasColumnType("numeric(18,2)");

        modelBuilder.Entity<ExpenseShare>()
            .Property(es => es.OwedAmount)
            .HasColumnType("numeric(18,2)");

        modelBuilder.Entity<RouteLocation>()
            .HasIndex(r => new { r.EventId, r.OrderIndex })
            .IsUnique();
    }
}