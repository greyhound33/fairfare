namespace FairFare.Domain.Entities;

public class ExpenseShare
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExpenseId { get; set; }
    public Guid DebtorId { get; set; }
    public decimal OwedAmount { get; set; }
    public Expense? Expense { get; set; }
    public Participant? Debtor { get; set; }
}