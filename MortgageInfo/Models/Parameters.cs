namespace MortgageInfo.Models;

public readonly record struct Parameters
{
    public decimal LoanAmount { get; init; }
    public float InterestRate { get; init; }
    public ushort Years { get; init; }
}
