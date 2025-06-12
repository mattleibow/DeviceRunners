using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace DeviceRunners.VisualRunners;

public class TcpResultChannel : IResultChannel
{
	readonly object _locker = new object();

	readonly ILogger<TcpResultChannel>? _logger;
	readonly IResultChannelFormatter _formatter;
	readonly bool _required;
	readonly int _retries;
	readonly TimeSpan _retryTimeout;
	readonly TimeSpan _timeout;

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
		_retries = options.Retries;
		_retryTimeout = options.RetryTimeout;
		_timeout = options.Timeout;

		HostNames = options.HostNames?.ToList() ?? [options.HostName];
		Port = options.Port;
		_logger = logger;
	}

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
		var hostName = await SelectBestHostName() ?? HostNames[0];

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

	async Task<string?> SelectBestHostName()
	{
		if (HostNames is null || HostNames.Count == 0)
			return null;
			
		if (HostNames.Count == 1)
			return HostNames[0];

		using var cts = new CancellationTokenSource();
		
		// Fire off all connection attempts concurrently
		var connectionTasks = new Dictionary<Task<bool>, string>();
		foreach (var hostName in HostNames)
		{
			var task = TryConnectToHost(hostName, cts.Token);
			connectionTasks.Add(task, hostName);
		}
		
		try
		{
			// Process tasks as they complete, waiting for success or until all fail
			while (connectionTasks.Count > 0)
			{
				// Find the first task that completes
				var completedTask = await Task.WhenAny(connectionTasks.Keys);
				
				// Get the hostname associated with this task and remove it from the dictionary
				var hostName = connectionTasks[completedTask];
				connectionTasks.Remove(completedTask);
				
				// Check if the connection was successful
				var connected = await completedTask;
				if (connected)
				{
					// Connection was successful, cancel all other attempts
					cts.Cancel();
					_logger?.LogInformation("Successfully connected to host {HostName}.", hostName);
					return hostName;
				}
				
				// This task failed, continue checking others
			}
			
			// All tasks completed without success
			_logger?.LogError("Unable to reach any host after trying all hosts.");
			return null;
		}
		finally
		{
			// Ensure we cancel any pending tasks when exiting the method
			if (!cts.IsCancellationRequested)
			{
				cts.Cancel();
			}
		}
	}
	
	async Task<bool> TryConnectToHost(string hostName, CancellationToken cancellationToken)
	{
		_logger?.LogInformation("Attempting to connect to {HostName}:{Port} with timeout {Timeout}...", hostName, Port, _timeout);

		for (var attempt = 0; attempt <= _retries; attempt++)
		{
			if (cancellationToken.IsCancellationRequested)
				return false;

			try
			{
				if (attempt > 0)
				{
					_logger?.LogInformation(
						"Retry attempt {Attempt} of {MaxRetries} for {HostName}:{Port} after waiting {RetryTimeout}...",
						attempt, _retries, hostName, Port, _retryTimeout);

					await Task.Delay(_retryTimeout, cancellationToken);
				}

				using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				timeoutCts.CancelAfter(_timeout);

				using var client = new TcpClient();
				await client.ConnectAsync(hostName, Port, timeoutCts.Token);

				using var writer = new StreamWriter(client.GetStream());
				await writer.WriteLineAsync("ping");
				await writer.FlushAsync();

				_logger?.LogInformation(
					"Connected to {HostName}:{Port} on attempt {Attempt}.",
					hostName, Port, attempt + 1);

				return true;
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				// This was a timeout, not a cancellation from outside
				_logger?.LogInformation(
					"Connection attempt to {HostName}:{Port} timed out after {Timeout}.",
					hostName, Port, _timeout);
			}
			catch (OperationCanceledException)
			{
				// This was a direct cancellation request, not a timeout
				return false;
			}
			catch (Exception ex) when (attempt == _retries)
			{
				_logger?.LogWarning(ex,
					"Failed to connect to {HostName}:{Port} after {MaxRetries} attempt(s).",
					hostName, Port, _retries + 1);
			}
			catch (Exception ex)
			{
				_logger?.LogWarning(ex,
					"Failed to connect to {HostName}:{Port} on attempt {Attempt}.",
					hostName, Port, attempt + 1);
			}
		}
		
		return false;
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
