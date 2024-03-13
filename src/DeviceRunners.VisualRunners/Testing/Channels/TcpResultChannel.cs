using System.Net.Sockets;

namespace DeviceRunners.VisualRunners;

public class TcpResultChannel : IResultChannel
{
	readonly object _locker = new object();

	readonly IResultChannelFormatter _formatter;
	readonly bool _required;

	TcpClient? _client;
	Stream? _stream;
	TextWriter? _writer;

	public TcpResultChannel(string hostName, int port, IResultChannelFormatter formatter, bool required = true)
		: this([hostName], port, formatter, required)
	{
		HostName = hostName;
	}

	public TcpResultChannel(IEnumerable<string> hostNames, int port, IResultChannelFormatter formatter, bool required = true)
	{
		if ((port < 0) || (port > ushort.MaxValue))
			throw new ArgumentOutOfRangeException(nameof(port));

		var names = hostNames?.ToList() ?? throw new ArgumentNullException(nameof(hostNames));
		if (names.Count == 0)
			throw new ArgumentException("At least one host name must be provided", nameof(hostNames));

		_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
		_required = required;

		HostNames = names;
		Port = port;
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
		HostName ??= SelectBestHostName() ?? HostNames[0];

		try
		{
			await _client.ConnectAsync(HostName, Port);
		}
		catch (Exception) when (!_required)
		{
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
							evt.Set();
					}
					catch (Exception)
					{
						if (Interlocked.Increment(ref failures) == HostNames.Count)
							evt.Set();
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
