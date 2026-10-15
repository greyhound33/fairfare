namespace FairFare.Domain.Entities;

public class Participant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid EventId { get; set; }
    public TripEvent? Event { get; set; }
    public List<Expense> PaidExpenses { get; set; } = new();
    public List<ExpenseShare> Debts { get; set; } = new();
}