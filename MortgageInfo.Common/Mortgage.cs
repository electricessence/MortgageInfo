using MortgageInfo.Common;
using Open.Collections;
using Throw;

namespace MortgageInfo.Console;

public class Mortgage
{
	private readonly double _monthlyInterestRate;
	private readonly decimal _monthlyRateDecimal;

	private readonly int _termMonths;

	public Parameters Parameters { get; }

	static decimal Trim(decimal value, decimal multiple = 100)
	{
		var result = Math.Ceiling(multiple * value) / multiple;
		result
			.Throw()
			.IfTrue(p => p * multiple % 1 != 0)
			.OnlyInDebug();

		return result;
	}

	static decimal Trim(double value, double multiple = 100)
	{
		var result = (decimal)(Math.Ceiling(multiple * value) / multiple);
		result
			.Throw()
			.IfTrue(p => p * (decimal)multiple % 1 != 0)
			.OnlyInDebug();

		return result;
	}

	public Mortgage(Parameters parameters)
	{
		Parameters = parameters;

		_monthlyInterestRate
			= parameters.InterestRate / 12;

		_monthlyRateDecimal = (decimal)_monthlyInterestRate;

		_termMonths
			= parameters.Years * 12;

		var i = _monthlyInterestRate;

		Payment
			= Trim(_monthlyInterestRate
				* (double)parameters.LoanAmount
				/ (1 - Math.Pow(1 + _monthlyInterestRate, -_termMonths)));

		Schedule
			= GetSchedule(parameters).MemoizeUnsafe();
	}

	public decimal Payment { get; }

	public LazyListUnsafe<Payment> Schedule { get; }

	private IEnumerable<Payment> GetSchedule(Parameters parameters)
	{
		var balance = parameters.LoanAmount;
		var interestTotal = 0M;
		var principalTotal = 0M;
		var years = parameters.Years;

		for (var year = 1; year <= years; year++)
		{
			for (var month = 1; month <= 12; month++)
			{
				var interest = Trim(balance * _monthlyRateDecimal);
				interestTotal += interest;

				var principal = Payment - interest;

				principal
					.Throw()
					.IfTrue(p => p * 100 % 1 != 0)
					.OnlyInDebug();

				principalTotal += principal;

				balance -= principal;

				yield return new Payment
				{
					Interest = new Paid { Current = interest, Total = interestTotal },
					Principal = new Paid { Current = principal, Total = principalTotal },
					Balance = balance,
				};
			}
		}
	}
}
