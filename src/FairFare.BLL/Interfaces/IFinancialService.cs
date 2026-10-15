using FairFare.Domain.Entities;
using FairFare.Domain.Models;

namespace FairFare.BLL.Interfaces;

public interface IFinancialService
{
    public ExpenseValidationResult ValidateExpense(TripEvent tripEvent, Guid payerId, decimal totalAmount, IEnumerable<Guid> debtorIds);
    public Expense AddExpense(TripEvent tripEvent, string description, decimal totalAmount, Guid payerId, IEnumerable<Guid> debtorIds);
    public IReadOnlyList<BalanceEntry> CalculateBalances(TripEvent tripEvent);
    public IReadOnlyList<Transfer> OptimizeTransfers(TripEvent tripEvent);
}
