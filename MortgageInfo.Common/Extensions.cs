using System.Text.Json;
using Throw;

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
}
