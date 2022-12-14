using Spectre.Console;
using System.Globalization;
using System.Numerics;
using Throw;

namespace MortgageInfo.Console;
internal static class Extensions
{
	public static T Prompt<T>(
		this IDictionary<string, string> store,
		string message,
		T defaultValue,
		Func<T, string>? converter = null)
		where T : notnull, INumber<T>
	{
		store.ThrowIfNull();
		message.ThrowIfNull();

		var value
			= store.TryGetValue(message, out var s)
			&& T.TryParse(s, NumberStyles.Any, null, out var v)
			? v : defaultValue;

		var defaultText = converter is null ? value.ToString().ThrowIfNull() : converter(value);
		var prompt = new TextPrompt<string>(message)
			.DefaultValue(defaultText)
			.Validate(response =>
			{
				if (T.TryParse(response, NumberStyles.Any, null, out value))
					return true;

				var span = response.AsSpan().Trim();
				if (!T.TryParse(span[..^1], NumberStyles.Any, null, out value))
					return false;

				dynamic m = span[^1] switch
				{
					'B' => 1_000_000_000,
					'M' => 1_000_000,
					'K' or 'k' => 1_000,
					_ => 0
				};

				if (m is 0) return false;

				value *= m;

				return true;
			});

		AnsiConsole.Prompt(prompt);
		store[message] = value.ToString().ThrowIfNull();

		return value;
	}

	public static TableColumn SetFooter(
		this TableColumn column,
		string text)
	{
		column.Footer = new Text(text);
		return column;
	}
}
