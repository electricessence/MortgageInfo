using System.Diagnostics;
using Open.Text;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using Throw;
using Spectre.Console;

namespace MortgageInfo.Console;
internal static class Extensions
{
	public static async ValueTask StoreAsync<T>(
		this FileInfo file,
		T data)
	{
		file.ThrowIfNull();
		if (data is null) throw new ArgumentNullException(nameof(data));

		using var fs = file.Open(FileMode.Create);
		await JsonSerializer.SerializeAsync(fs, data);
	}

	public static async ValueTask<T> RetrieveAsync<T>(
		this FileInfo file)
		where T : new()
	{
		if (!file.Exists) return new T();
		using var fs = file.Open(FileMode.Open);
		return await JsonSerializer.DeserializeAsync<T>(fs) ?? new T();
	}

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

		var prompt = new TextPrompt<T>(message).DefaultValue(value);
		prompt.Converter = converter;
		var input = AnsiConsole.Prompt(prompt);
		store[message] = input.ToString().ThrowIfNull();

		return value;
	}
}
