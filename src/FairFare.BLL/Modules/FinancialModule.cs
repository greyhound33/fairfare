using FairFare.Domain.Entities;
using FairFare.Domain.Models;

namespace FairFare.BLL.Modules;

public class FinancialModule
{
    public bool ValidateExpense(Expense expense, out string errorMessage)
    {
        if (expense.TotalAmount <= 0)
        {
            errorMessage = "Total amount must be greater than zero.";
            return false;
        }

        if (!expense.Shares.Any())
        {
            errorMessage = "At least one debtor must be assigned.";
            return false;
        }

        decimal totalShares = expense.Shares.Sum(s => s.OwedAmount);
        if (totalShares != expense.TotalAmount)
        {
            errorMessage = "Sum of debtor shares must equal TotalAmount.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public List<Transfer> OptimizePayments(List<Expense> expenses, List<Participant> participants)
    {
        return new List<Transfer>();
    }
}