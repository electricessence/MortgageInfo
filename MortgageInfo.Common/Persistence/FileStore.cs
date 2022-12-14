namespace MortgageInfo.Console.Persistence;

public class FileStore<T> : IStore<T>
	where T : new()
{
	private readonly FileInfo _file;

	public FileStore(FileInfo file, T data)
	{
		_file = file;
		Data = data;
	}

	public FileStore(string path, T data)
		: this(new FileInfo(path), data) { }

	public static async ValueTask<FileStore<T>> CreateAsync(string path)
	{
		var file = new FileInfo(path);
		var data = await file.RetrieveAsync<T>();
		return new FileStore<T>(file, data);
	}

	public T Data { get; }

	public ValueTask<T> RetrieveAsync()
		=> _file.RetrieveAsync<T>();

	public ValueTask SaveAsync()
		=> _file.StoreAsync(Data);
}
