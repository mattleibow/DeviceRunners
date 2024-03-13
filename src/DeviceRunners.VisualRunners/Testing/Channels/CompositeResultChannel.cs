namespace DeviceRunners.VisualRunners;

public class CompositeResultChannel : IResultChannel
{
	readonly IReadOnlyList<IResultChannel> _resultChannels;

	public CompositeResultChannel(IEnumerable<IResultChannel> resultChannels)
	{
		_resultChannels = resultChannels.ToList();
	}

	public bool IsOpen { get; private set; }

	public async Task CloseChannel()
	{
		foreach (var channel in _resultChannels)
		{
			await channel.CloseChannel();
		}

		IsOpen = false;
	}

	public async Task<bool> OpenChannel(string? message = null)
	{
		var success = true;
		foreach (var channel in _resultChannels)
		{
			var s = await channel.OpenChannel(message);
			success = success || s;
		}
		IsOpen = true;
		return success;
	}

	public void RecordResult(ITestResultInfo testResult)
	{
		foreach (var channel in _resultChannels)
		{
			channel.RecordResult(testResult);
		}
	}
}
