using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3DiscoveryMessageSink : IMessageSink
{
	readonly CancellationToken _cancellationToken;

	public Xunit3DiscoveryMessageSink(CancellationToken cancellationToken = default)
	{
		_cancellationToken = cancellationToken;
	}

	public ManualResetEventSlim Finished { get; } = new(false);

	public List<ITestCaseDiscovered> TestCases { get; } = new();

	public List<string> AllMessages { get; } = new();

	public bool OnMessage(IMessageSinkMessage message)
	{
		AllMessages.Add(message.GetType().Name);

		if (message is ITestCaseDiscovered testCaseDiscovered)
		{
			TestCases.Add(testCaseDiscovered);
		}
		else if (message is IDiscoveryComplete)
		{
			Finished.Set();
		}

		return !_cancellationToken.IsCancellationRequested;
	}
}
