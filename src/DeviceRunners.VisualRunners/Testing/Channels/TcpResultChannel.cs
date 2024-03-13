using System.Net.Sockets;

namespace DeviceRunners.VisualRunners;

public class TcpResultChannel : IResultChannel
{
	readonly object _locker = new object();

	readonly IResultChannelFormatter _formatter;

	TcpClient? _client;
	Stream? _stream;
	TextWriter? _writer;

	public TcpResultChannel(string hostName, int port, IResultChannelFormatter formatter)
	{
		if ((port < 0) || (port > ushort.MaxValue))
			throw new ArgumentOutOfRangeException(nameof(port));

		_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

		HostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
		Port = port;
	}

	public string HostName { get; }

	public int Port { get; }

	public bool IsOpen => _stream is not null;

	public async Task<bool> OpenChannel(string? message = null)
	{
		lock (_locker)
		{
			_client = new TcpClient();
		}

		await _client.ConnectAsync(HostName, Port);

		lock (_locker)
		{
			_stream = _client.GetStream();
			_writer = new StreamWriter(_stream);

			_formatter.BeginTestRun(_writer, message);

			_writer.Flush();
		}

		return true;
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
