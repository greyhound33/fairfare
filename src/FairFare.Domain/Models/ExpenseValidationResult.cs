namespace FairFare.Domain.Models;

public sealed class ExpenseValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}