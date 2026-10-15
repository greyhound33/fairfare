namespace FairFare.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid PayerId { get; set; }
    public Guid EventId { get; set; }
    public TripEvent? Event { get; set; }
    public Participant? Payer { get; set; }
    public List<ExpenseShare> Shares { get; set; } = new();
}