namespace FairFare.Domain.Entities;

public class Expense
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int PayerId { get; set; }
    public Participant? Payer { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public ICollection<ExpenseShare> Shares { get; set; } = new List<ExpenseShare>();
}
