
using MoreLinq;
using MortgageInfo.Common;
using MortgageInfo.Console;
using MortgageInfo.Console.Persistence;
using Spectre.Console;

var storage = await FileStore<Dictionary<string, string>>.CreateAsync("params.json");
var data = storage.Data;

AnsiConsole.Write(new Rule("Mortgage Details"));
///////////////////////////////////////////////////
var results = new Mortgage(GetParameters());
Summary(results);
var taxRate = data.NumericPrompt("What is your marginal tax rate?", 25.0M, v => $"{v:0.0}%") / 100;
await storage.SaveAsync();
ShowSchedule(results.Schedule);
///////////////////////////////////////////////////

Parameters GetParameters() => new()
{
	LoanAmount = data.NumericPrompt("What is the loan amount?", 500_000.0M, v => v.ToString("C0")),
	InterestRate = data.NumericPrompt("What is interest rate?", 5.0f, v => $"{v:0.000}%") / 100,
	Years = data.NumericPrompt<ushort>("How many years?", 30)
};

static void Summary(Mortgage results)
{
	AnsiConsole.Write(new Rule("Summary"));
	AnsiConsole.WriteLine("Monthly Payment: {0:C2}", results.Payment);
	AnsiConsole.WriteLine("Interest: {0:C2} after {1} years",
		results.Schedule.Last().Interest.Total,
		results.Parameters.Years);
	AnsiConsole.Write(new Rule());
}

decimal TaxSavings(decimal interest) => taxRate * interest;

void ShowSchedule(IEnumerable<Payment> results)
{
	AnsiConsole.Write(new Rule());
	var year = 0;
	foreach (var payments in results.Batch(12))
	{
		year++;
		// Always show the first year.
		if (year != 1)
		{
			var show = AnsiConsole.Confirm($"Show year {year}?");
			AnsiConsole.Cursor.MoveUp(1);
			if (!show) break;
		}

		var table = new Table
		{
			Title = new TableTitle($"Year {year:00}")
		};

		var intCol = new TableColumn("Interest").RightAligned();
		var prinCol = new TableColumn("Principal").RightAligned();

		table
			.AddColumn(
				new TableColumn("Month")
				.SetFooter("Total:")
				.RightAligned())
			.AddColumn(intCol)
			.AddColumn(prinCol);

		var month = 0;
		var interest = 0M;
		var principal = 0M;
		foreach (var payment in payments)
		{
			month++;
			table.AddRow(
				month.ToString("00 "),
				payment.Interest.Current.ToString("C2"),
				payment.Principal.Current.ToString("C2"));
			interest += payment.Interest.Current;
			principal += payment.Principal.Current;
		}

		intCol.SetFooter(interest.ToString("C2"));
		prinCol.SetFooter(principal.ToString("C2"));
		table.Caption = new TableTitle($"Tax Savings: {TaxSavings(interest):C2}");

		AnsiConsole.Write(table);
		AnsiConsole.WriteLine();
	}

	AnsiConsole.WriteLine();
	AnsiConsole.WriteLine();
}