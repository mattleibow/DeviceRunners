using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestHost;

using DeviceRunners.VisualRunners;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// MTP IDataConsumer that writes TestResultEvent as NDJSON lines to Console.Out.
/// Used in WASM where TCP is not available — the CLI captures console.log via CDP.
/// Same wire format as TcpStreamingConsumer, just different transport.
/// </summary>
sealed class ConsoleStreamingConsumer : IDataConsumer
{
	readonly SemaphoreSlim _writeLock = new(1, 1);
	bool _sentBegin;

	public string Uid => "DeviceRunners.ConsoleStreamingConsumer";
	public string Version => "1.0.0";
	public string DisplayName => "Console Streaming Consumer";
	public string Description => "Streams test events to console for WASM/CDP capture";

	public Task<bool> IsEnabledAsync() => Task.FromResult(true);

	public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];

	public async Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
	{
		if (value is not TestNodeUpdateMessage message)
			return;

		await _writeLock.WaitAsync(cancellationToken);
		try
		{
			if (!_sentBegin)
			{
				Console.WriteLine(TestResultEvent.Begin().ToString());
				_sentBegin = true;
			}

			var evt = TestNodeMapper.ToTestResultEvent(message);
			Console.WriteLine(evt.ToString());
		}
		finally
		{
			_writeLock.Release();
		}
	}
}
