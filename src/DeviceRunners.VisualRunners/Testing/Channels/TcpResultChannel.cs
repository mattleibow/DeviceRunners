using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace DeviceRunners.VisualRunners;

public class TcpResultChannel : IResultChannel
{
	readonly object _locker = new object();

	readonly ILogger<TcpResultChannel>? _logger;
	readonly IResultChannelFormatter _formatter;
	readonly bool _required;

	TcpClient? _client;
	Stream? _stream;
	TextWriter? _writer;

	public TcpResultChannel(TcpResultChannelOptions options, ILogger<TcpResultChannel>? logger)
	{
		if ((options.Port < 0) || (options.Port > ushort.MaxValue))
			throw new ArgumentOutOfRangeException(nameof(options), $"Port must be in the range of [0..{ushort.MaxValue}].");

		if (options.HostName is null && (options.HostNames is null || options.HostNames?.Count == 0))
			throw new ArgumentException("At least one host name must be provided.", nameof(options));

		_formatter = options.Formatter ?? throw new ArgumentNullException(nameof(options.Formatter));
		_required = options.Required;

		HostNames = options.HostNames?.ToList() ?? [options.HostName];
		Port = options.Port;
		_logger = logger;
	}

	public string? HostName { get; private set; }

	public IReadOnlyList<string> HostNames { get; }

	public int Port { get; }

	public bool IsOpen => _writer is not null;

	public async Task<bool> OpenChannel(string? message = null)
	{
		lock (_locker)
		{
			_client = new TcpClient();
		}

		// no host was selected, so try them all and then fallback to the first one
		var hostName = HostName ?? SelectBestHostName() ?? HostNames[0];

		_logger?.LogInformation("Connecting to {HostName}:{Port}...", hostName, Port);

		try
		{
			await _client.ConnectAsync(hostName, Port);
		}
		catch (Exception ex) when (!_required)
		{
			_logger?.LogError(ex, "Failed to connect to {HostName}:{Port}.", hostName, Port);

			return false;
		}

		lock (_locker)
		{
			_stream = _client.GetStream();
			_writer = new StreamWriter(_stream);

			_formatter.BeginTestRun(_writer, message);

			_writer.Flush();
		}

		return true;
	}

	string? SelectBestHostName()
	{
		// If there's only one host, there's no need to select
		if (HostNames.Count == 1)
			return null;

		// If there's more than one host, we need to select the best/first good one
		var tcs = new CancellationTokenSource();
		var selected = -1;
		var failures = 0;

		using (var evt = new ManualResetEventSlim(false))
		{
			for (var i = 0; i < HostNames.Count; i++)
			{
				var name = HostNames[i];
				var idx = i;

				_logger?.LogInformation("Pinging {HostName}:{Port}...", name, Port);

				Task.Run(async () =>
				{
					try
					{
						var client = new TcpClient();
						await client.ConnectAsync(name, Port, tcs.Token);
						using (var writer = new StreamWriter(client.GetStream()))
						{
							writer.WriteLine("ping");
						}

						if (Interlocked.CompareExchange(ref selected, idx, -1) == -1)
						{
							_logger?.LogInformation("Connected to {HostName}:{Port}.", name, Port);
							evt.Set();
						}
					}
					catch (Exception ex)
					{
						if (Interlocked.Increment(ref failures) == HostNames.Count)
						{
							_logger?.LogWarning(ex, "Unable to reach {HostName}:{Port}.", name, Port);
							evt.Set();
						}
					}
				});
			}

			// Wait for 1 success or all failures
			evt.Wait();

			// Cancel all the other pending ones
			tcs.Cancel();
		}

		return selected == -1 ? null : HostNames[selected];
	}

	public void RecordResult(ITestResultInfo testResult)
	{
		lock (_locker)
		{
			if (_writer is null)
				return;

			_formatter.RecordResult(testResult);

			_writer.Flush();
		}
	}

	public async Task CloseChannel()
	{
		lock (_locker)
		{
			if (_writer is null)
				return;

			_formatter.EndTestRun();

			_writer.Flush();
		}

		await _writer.DisposeAsync();

		_writer = null;
		_stream = null;
		_client = null;
	}
}
