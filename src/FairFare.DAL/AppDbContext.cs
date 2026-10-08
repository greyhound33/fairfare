using FairFare.Domain;
using Microsoft.EntityFrameworkCore;

namespace FairFare.DAL;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TripEvent> Events => Set<TripEvent>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseShare> ExpenseShares => Set<ExpenseShare>();
    public DbSet<RouteLocation> RouteLocations => Set<RouteLocation>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TripEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.HasMany(e => e.Participants)
                .WithOne(p => p.Event)
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Expenses)
                .WithOne(e => e.Event)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.RouteLocations)
                .WithOne(r => r.Event)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Reports)
                .WithOne(r => r.Event)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(150).IsRequired();
            entity.HasOne(p => p.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasMany(e => e.Shares)
                .WithOne(s => s.Expense)
                .HasForeignKey(s => s.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExpenseShare>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.OwedAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(s => s.Debtor)
                .WithMany(p => p.Debts)
                .HasForeignKey(s => s.DebtorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RouteLocation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.LocationName).HasMaxLength(150).IsRequired();
            entity.HasIndex(r => new { r.EventId, r.OrderIndex }).IsUnique();
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Title).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Summary).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
