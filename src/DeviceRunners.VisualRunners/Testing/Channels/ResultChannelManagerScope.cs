namespace DeviceRunners.VisualRunners;

public class ResultChannelManagerScope : IAsyncDisposable
{
	readonly IResultChannelManager? _manager;
	bool _weOpened;

	ResultChannelManagerScope(IResultChannelManager? manager)
	{
		_manager = manager;
	}

	public async Task EnsureOpenAsync()
	{
		if (_manager is null || _manager.IsOpen)
			return;

		await _manager.OpenChannel();

		_weOpened = true;
	}

	public async ValueTask DisposeAsync()
	{
		if (_manager is null)
			return;

		if (_weOpened && _manager.IsOpen)
		{
			await _manager.CloseChannel();
		}
	}

	public static async Task<IAsyncDisposable> OpenAsync(IResultChannelManager? manager)
	{
		var closing = new ResultChannelManagerScope(manager);
		await closing.EnsureOpenAsync();
		return closing;
	}
}
