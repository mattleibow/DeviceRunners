using System.Net.Sockets;

using Microsoft.Extensions.Logging;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestHost;

using DeviceRunners.VisualRunners;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// MTP IDataConsumer that receives TestNodeUpdateMessage and streams them
/// over TCP as NDJSON TestResultEvent lines. Thread-safe via SemaphoreSlim.
/// </summary>
sealed class TcpStreamingConsumer : IDataConsumer, IAsyncDisposable
{
	readonly TcpStreamingConsumerOptions _options;
	readonly ILogger? _logger;
	readonly SemaphoreSlim _writeLock = new(1, 1);
	TcpClient? _client;
	StreamWriter? _writer;
	bool _sentBegin;

	public TcpStreamingConsumer(TcpStreamingConsumerOptions options, ILogger? logger = null)
	{
		_options = options;
		_logger = logger;
	}

	public string Uid => "DeviceRunners.TcpStreamingConsumer";
	public string Version => "1.0.0";
	public string DisplayName => "TCP Streaming Consumer";
	public string Description => "Streams test events over TCP to DeviceRunners host";

	public Task<bool> IsEnabledAsync() => Task.FromResult(true);

	public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];

	public async Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
	{
		if (value is not TestNodeUpdateMessage message)
			return;

		await _writeLock.WaitAsync(cancellationToken);
		try
		{
			await EnsureConnectedAsync(cancellationToken);

			if (!_sentBegin)
			{
				var beginLine = TestResultEvent.Begin().ToString();
				await _writer!.WriteLineAsync(beginLine);
				await _writer.FlushAsync(cancellationToken);
				_sentBegin = true;
			}

			var evt = TestNodeMapper.ToTestResultEvent(message);
			var line = evt.ToString();
			await _writer!.WriteLineAsync(line);
			await _writer.FlushAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger?.LogWarning(ex, "Failed to stream test event over TCP");
		}
		finally
		{
			_writeLock.Release();
		}
	}

	async Task EnsureConnectedAsync(CancellationToken cancellationToken)
	{
		if (_client?.Connected == true)
			return;

		_client?.Dispose();
		_client = null;
		_writer = null;

		for (var attempt = 0; attempt <= _options.Retries; attempt++)
		{
			foreach (var host in _options.HostNames)
			{
				try
				{
					var client = new TcpClient();
					using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
					cts.CancelAfter(_options.ConnectionTimeout);

					await client.ConnectAsync(host, _options.Port, cts.Token);
					_client = client;
					_writer = new StreamWriter(client.GetStream()) { AutoFlush = false };
					_logger?.LogInformation("Connected to {Host}:{Port}", host, _options.Port);
					return;
				}
				catch (Exception ex) when (ex is not OperationCanceledException)
				{
					_logger?.LogDebug(ex, "Connection attempt to {Host}:{Port} failed", host, _options.Port);
				}
			}

			if (attempt < _options.Retries)
			{
				_logger?.LogDebug("Retrying connection in {Timeout}...", _options.RetryTimeout);
				await Task.Delay(_options.RetryTimeout, cancellationToken);
			}
		}

		throw new InvalidOperationException(
			$"Failed to connect to any host ({string.Join(", ", _options.HostNames)}) on port {_options.Port} after {_options.Retries + 1} attempts");
	}

	public async ValueTask DisposeAsync()
	{
		await _writeLock.WaitAsync();
		try
		{
			if (_writer is not null && _sentBegin)
			{
				try
				{
					var endLine = TestResultEvent.End().ToString();
					await _writer.WriteLineAsync(endLine);
					await _writer.FlushAsync();
				}
				catch
				{
					// Best effort
				}
			}

			_writer?.Dispose();
			_client?.Dispose();
		}
		finally
		{
			_writeLock.Release();
		}
	}
}
