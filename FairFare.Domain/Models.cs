namespace FairFare.Domain;

public class TripEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<Participant> Participants { get; set; } = new();
    public List<Expense> Expenses { get; set; } = new();
    public List<RouteLocation> RouteLocations { get; set; } = new();
    public List<Report> Reports { get; set; } = new();
}

public class Participant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid EventId { get; set; }
    public TripEvent? Event { get; set; }
    public List<Expense> PaidExpenses { get; set; } = new();
    public List<ExpenseShare> Debts { get; set; } = new();
}

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

public class ExpenseShare
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExpenseId { get; set; }
    public Guid DebtorId { get; set; }
    public decimal OwedAmount { get; set; }
    public Expense? Expense { get; set; }
    public Participant? Debtor { get; set; }
}

public class RouteLocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string LocationName { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public Guid EventId { get; set; }
    public TripEvent? Event { get; set; }
}

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid EventId { get; set; }
    public TripEvent? Event { get; set; }
}

public class Transfer
{
    public Guid FromParticipantId { get; set; }
    public Guid ToParticipantId { get; set; }
    public decimal Amount { get; set; }
}

public class BalanceEntry
{
    public Guid ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class ExpenseValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}
