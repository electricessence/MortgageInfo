namespace MortgageInfo.Console.Persistence;
public interface IStore<T>
	where T : new()
{
	ValueTask SaveAsync();

	ValueTask<T> RetrieveAsync();
}
