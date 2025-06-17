using Microsoft.Extensions.Logging;

namespace DeviceRunners.VisualRunners;

public class DefaultResultChannelManager : IResultChannelManager
{
	readonly IReadOnlyList<IResultChannel> _resultChannels;
	readonly ILogger<DefaultResultChannelManager>? _logger;

	public DefaultResultChannelManager()
	{
		_resultChannels = [];
	}

	public DefaultResultChannelManager(IEnumerable<IResultChannel> resultChannels, ILogger<DefaultResultChannelManager>? logger = null)
	{
		_resultChannels = resultChannels.ToList();
		_logger = logger;
	}

	public bool IsOpen { get; set; }

	public async Task<bool> OpenChannel(string? message = null)
	{
		_logger?.LogInformation("Opening {Count} result channels...", _resultChannels.Count);

		foreach (var channel in _resultChannels)
		{
			if (!channel.IsOpen)
			{
				_logger?.LogInformation("Opening channel: {ChannelType}", channel.GetType().Name);
				try
				{
					await channel.OpenChannel(message);
				}
				catch (Exception ex)
				{
					_logger?.LogError(ex, "Failed to open channel: {ChannelType}", channel.GetType().Name);
				}
			}
			else
			{
				_logger?.LogInformation("Channel {ChannelType} is already open.", channel.GetType().Name);
			}
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
