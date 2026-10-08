namespace FairFare.Domain.Entities;

public class ExpenseShare
{
    public int Id { get; set; }
    public decimal OwedAmount { get; set; }
    public int ExpenseId { get; set; }
    public Expense? Expense { get; set; }
    public int DebtorId { get; set; }
    public Participant? Debtor { get; set; }
}