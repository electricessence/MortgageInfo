
using MoreLinq;
using MortgageInfo.Common;
using MortgageInfo.Console;
using MortgageInfo.Console.Persistence;
using Spectre.Console;

AnsiConsole.Write(new Rule("Mortgage Details"));
///////////////////////////////////////////////////
var results = new Mortgage(await GetParameters());
Summary(results);
ShowSchedule(results.Schedule);
///////////////////////////////////////////////////

static async ValueTask<Parameters> GetParameters()
{
	var storage = await FileStore<Dictionary<string, string>>.CreateAsync("params.json");
	var data = storage.Data;

	var p = new Parameters
	{
		LoanAmount = data.Prompt("What is the loan amount?", 500_000.0M, v => v.ToString("C0")),
		InterestRate = data.Prompt("What is interest rate?", 5.0f) / 100,
		Years = data.Prompt<ushort>("How many years?", 30)
	};

	await storage.SaveAsync();

	return p;
}

static void Summary(Mortgage results)
{
	AnsiConsole.Write(new Rule("Summary"));
	AnsiConsole.WriteLine("Monthly Payment: {0:C2}", results.Payment);
	AnsiConsole.WriteLine("End of Term Interest: {0:C2}", results.Schedule.Last().Interest.Total);
	AnsiConsole.Write(new Rule());
}

static void ShowSchedule(IEnumerable<Payment> results)
{
	var year = 0;
	foreach (var payments in results.Batch(12))
	{
		if (!AnsiConsole.Confirm($"Show year {++year}?")) break;
		AnsiConsole.Cursor.MoveUp(1);

		var table = new Table
		{
			Title = new TableTitle($"Year {year:00}")
		};
		table.AddColumn("Month");
		table.AddColumn("Interest");
		table.AddColumn("Principal");

		var month = 0;
		foreach (var payment in payments)
		{
			month++;
			table.AddRow(
				month.ToString("00"),
				payment.Interest.Current.ToString("C2"),
				payment.Principal.Current.ToString("C2"));
		}

		AnsiConsole.Write(table);
	}
}