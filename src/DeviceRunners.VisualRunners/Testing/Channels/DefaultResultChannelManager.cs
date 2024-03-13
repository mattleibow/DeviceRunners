namespace DeviceRunners.VisualRunners;

public class DefaultResultChannelManager : IResultChannelManager
{
	readonly IReadOnlyList<IResultChannel> _resultChannels;

	public DefaultResultChannelManager()
	{
		_resultChannels = [];
	}

	public DefaultResultChannelManager(IEnumerable<IResultChannel> resultChannels)
	{
		_resultChannels = resultChannels.ToList();
	}

	public bool IsOpen { get; set; }

	public async Task<bool> OpenChannel(string? message = null)
	{
		foreach (var channel in _resultChannels)
		{
			if (!channel.IsOpen)
				await channel.OpenChannel(message);
		}

		IsOpen = true;
		return IsOpen;
	}

	public async Task CloseChannel()
	{
		foreach (var channel in _resultChannels)
		{
			if (channel.IsOpen)
				await channel.CloseChannel();
		}

		IsOpen = false;
	}

	public void RecordResult(ITestResultInfo testResult)
	{
		foreach (var channel in _resultChannels)
		{
			if (channel.IsOpen)
				channel.RecordResult(testResult);
		}
	}
}
