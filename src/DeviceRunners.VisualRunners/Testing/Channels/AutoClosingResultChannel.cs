namespace DeviceRunners.VisualRunners;

public class AutoClosingResultChannel : IAsyncDisposable
{
	readonly IResultChannel? _channel;
	bool _weOpened;

	public AutoClosingResultChannel(IResultChannel? channel)
	{
		_channel = channel;
	}

	public async Task EnsureOpenAsync()
	{
		if (_channel is null || _channel.IsOpen)
			return;

		await _channel.OpenChannel();
		_weOpened = true;
	}

	public async ValueTask DisposeAsync()
	{
		if (_weOpened && _channel is not null && _channel.IsOpen)
		{
			await _channel.CloseChannel();
		}
	}
}
