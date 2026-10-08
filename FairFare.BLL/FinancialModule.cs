using FairFare.Domain;

namespace FairFare.BLL;

public sealed class FinancialModule
{
    public ExpenseValidationResult ValidateExpense(TripEvent tripEvent, Guid payerId, decimal totalAmount, IEnumerable<Guid> debtorIds)
    {
        if (tripEvent is null)
        {
            throw new ArgumentNullException(nameof(tripEvent));
        }

        var debtors = debtorIds.Distinct().ToList();

        if (!tripEvent.Participants.Any(participant => participant.Id == payerId))
        {
            return new ExpenseValidationResult { IsValid = false, Message = "Payer must be an existing participant." };
        }

        if (totalAmount <= 0)
        {
            return new ExpenseValidationResult { IsValid = false, Message = "Expense total must be greater than zero." };
        }

        if (debtors.Count == 0)
        {
            return new ExpenseValidationResult { IsValid = false, Message = "At least one participant must be selected for the split." };
        }

        if (debtors.Any(debtorId => !tripEvent.Participants.Any(participant => participant.Id == debtorId)))
        {
            return new ExpenseValidationResult { IsValid = false, Message = "Every debtor must belong to the trip." };
        }

        return new ExpenseValidationResult { IsValid = true, Message = string.Empty };
    }

    public Expense AddExpense(TripEvent tripEvent, string description, decimal totalAmount, Guid payerId, IEnumerable<Guid> debtorIds)
    {
        if (tripEvent is null)
        {
            throw new ArgumentNullException(nameof(tripEvent));
        }

        var debtorList = debtorIds.Distinct().ToList();
        var validation = ValidateExpense(tripEvent, payerId, totalAmount, debtorList);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.Message);
        }

        var expense = new Expense
        {
            Description = string.IsNullOrWhiteSpace(description) ? "Trip expense" : description.Trim(),
            TotalAmount = totalAmount,
            PayerId = payerId,
            EventId = tripEvent.Id,
            Event = tripEvent,
            Payer = tripEvent.Participants.Single(participant => participant.Id == payerId)
        };

        var splitAmount = totalAmount / debtorList.Count;
        foreach (var debtorId in debtorList)
        {
            expense.Shares.Add(new ExpenseShare
            {
                ExpenseId = expense.Id,
                Expense = expense,
                DebtorId = debtorId,
                Debtor = tripEvent.Participants.Single(participant => participant.Id == debtorId),
                OwedAmount = splitAmount
            });
        }

        tripEvent.Expenses.Add(expense);
        return expense;
    }

    public IReadOnlyList<BalanceEntry> CalculateBalances(TripEvent tripEvent)
    {
        if (tripEvent is null)
        {
            throw new ArgumentNullException(nameof(tripEvent));
        }

        var balances = tripEvent.Participants.ToDictionary(participant => participant.Id, _ => 0m);

        foreach (var expense in tripEvent.Expenses)
        {
            if (!balances.ContainsKey(expense.PayerId))
            {
                continue;
            }

            balances[expense.PayerId] += expense.TotalAmount;

            foreach (var share in expense.Shares)
            {
                if (!balances.ContainsKey(share.DebtorId))
                {
                    continue;
                }

                balances[share.DebtorId] -= share.OwedAmount;
            }
        }

        return tripEvent.Participants
            .Select(participant => new BalanceEntry
            {
                ParticipantId = participant.Id,
                ParticipantName = participant.Name,
                Amount = balances[participant.Id]
            })
            .OrderBy(entry => entry.ParticipantName)
            .ToList();
    }

    public IReadOnlyList<Transfer> OptimizeTransfers(TripEvent tripEvent)
    {
        if (tripEvent is null)
        {
            throw new ArgumentNullException(nameof(tripEvent));
        }

        var balances = CalculateBalances(tripEvent)
            .ToDictionary(entry => entry.ParticipantId, entry => entry.Amount);

        var debtors = balances
            .Where(pair => pair.Value < 0m)
            .ToDictionary(pair => pair.Key, pair => -pair.Value);

        var creditors = balances
            .Where(pair => pair.Value > 0m)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        var transfers = new List<Transfer>();

        while (debtors.Count > 0 && creditors.Count > 0)
        {
            var debtorId = debtors
                .OrderByDescending(pair => pair.Value)
                .Select(pair => pair.Key)
                .First();

            var creditorId = creditors
                .OrderByDescending(pair => pair.Value)
                .Select(pair => pair.Key)
                .First();

            var transferAmount = Math.Min(debtors[debtorId], creditors[creditorId]);
            if (transferAmount <= 0m)
            {
                break;
            }

            transfers.Add(new Transfer
            {
                FromParticipantId = debtorId,
                ToParticipantId = creditorId,
                Amount = transferAmount
            });

            debtors[debtorId] -= transferAmount;
            creditors[creditorId] -= transferAmount;

            if (debtors[debtorId] <= 0m)
            {
                debtors.Remove(debtorId);
            }

            if (creditors[creditorId] <= 0m)
            {
                creditors.Remove(creditorId);
            }
        }

        return transfers;
    }
}
