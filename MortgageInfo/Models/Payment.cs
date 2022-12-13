namespace MortgageInfo.Models;

public readonly record struct Payment
{
    public Paid Interest { get; init; }
    public Paid Principal { get; init; }
    public decimal Balance { get; init; }
}
