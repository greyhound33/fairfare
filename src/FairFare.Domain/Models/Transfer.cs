namespace FairFare.Domain.Models;


public class Transfer
{
    public int FromParticipantId { get; set; }
    public int ToParticipantId { get; set; }
    public decimal Amount { get; set; }
}