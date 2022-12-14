namespace MortgageInfo.Common;

public readonly record struct Paid
{
	public decimal Current { get; init; }
	public decimal Total { get; init; }
}
