namespace FairFare.Domain.Models;

public class BalanceEntry
{
    public Guid ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}