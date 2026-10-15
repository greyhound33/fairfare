namespace FairFare.Domain.Entities;

public class Transfer
{
    public Guid FromParticipantId { get; set; }
    public Guid ToParticipantId { get; set; }
    public decimal Amount { get; set; }
}