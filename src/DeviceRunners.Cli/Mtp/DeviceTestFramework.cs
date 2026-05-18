using System.Net;
using System.Net.Sockets;
using System.Text;

using DeviceRunners.Cli.Services;
using DeviceRunners.VisualRunners;

using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.TestHost;

namespace DeviceRunners.Cli.Mtp;

/// <summary>
/// ITestFramework implementation that runs on the HOST machine.
/// When dotnet test calls ExecuteRequestAsync, this:
/// 1. Starts a TCP listener
/// 2. Launches the device app with env vars (DEVICE_RUNNERS_AUTORUN=1, etc.)
/// 3. Listens for NDJSON TestResultEvent lines over TCP
/// 4. Maps each event → TestNodeUpdateMessage
/// 5. Publishes to context.MessageBus
/// 6. Calls context.Complete() when TCP closes
/// </summary>
sealed class DeviceTestFramework : ITestFramework, IDataProducer
{
	readonly IServiceProvider _serviceProvider;
	readonly string[] _args;

	public DeviceTestFramework(IServiceProvider serviceProvider, string[] args)
	{
		_serviceProvider = serviceProvider;
		_args = args;
	}

	// IExtension
	public string Uid => "DeviceRunners.DeviceTestFramework";
	public string Version => "1.0.0";
	public string DisplayName => "Device Test Framework";
	public string Description => "Runs tests on physical devices and emulators via DeviceRunners";
	public Task<bool> IsEnabledAsync() => Task.FromResult(true);

	// IDataProducer
	public Type[] DataTypesProduced => [typeof(TestNodeUpdateMessage)];

	public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context)
		=> Task.FromResult(new CreateTestSessionResult { IsSuccess = true });

	public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context)
		=> Task.FromResult(new CloseTestSessionResult { IsSuccess = true });

	public async Task ExecuteRequestAsync(ExecuteRequestContext context)
	{
		try
		{
			var isDiscovery = context.Request is DiscoverTestExecutionRequest;

			// Parse platform args from the CLI invocation
			var platformArgs = DeviceTestFrameworkArgs.Parse(_args);

			// Start TCP listener
			var port = platformArgs.Port;
			using var listener = new TcpListener(IPAddress.Any, port);
			listener.Start();

			try
			{
				// TODO: Launch app on device using platform services
				// For now, assume the app is already running or will be launched
				// by the MSBuild targets before this is called.
				// Future: integrate with AndroidService, iOSService, etc.

				// Wait for connection with timeout
				using var connectionCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
				connectionCts.CancelAfter(TimeSpan.FromSeconds(platformArgs.ConnectionTimeout));

				while (!listener.Pending())
				{
					connectionCts.Token.ThrowIfCancellationRequested();
					await Task.Delay(100, connectionCts.Token);
				}

				using var client = await listener.AcceptTcpClientAsync(context.CancellationToken);
				using var stream = client.GetStream();

				// Read NDJSON events
				var buffer = new byte[4096];
				var lineBuffer = new StringBuilder();
				int bytesRead;

				using var dataCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
				dataCts.CancelAfter(TimeSpan.FromSeconds(platformArgs.DataTimeout));

				while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(), dataCts.Token)) > 0)
				{
					// Reset data timeout on each chunk
					dataCts.CancelAfter(TimeSpan.FromSeconds(platformArgs.DataTimeout));

					var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					lineBuffer.Append(data);

					// Extract complete lines
					var buffered = lineBuffer.ToString();
					var lastNewline = buffered.LastIndexOf('\n');
					if (lastNewline < 0)
						continue;

					var completeData = buffered[..lastNewline];
					lineBuffer.Clear();
					lineBuffer.Append(buffered[(lastNewline + 1)..]);

					foreach (var line in completeData.Split('\n'))
					{
						var trimmedLine = line.TrimEnd('\r');
						if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine == "ping")
							continue;

						var evt = TestResultEvent.Parse(trimmedLine);
						if (evt is null)
							continue;

						await ProcessEventAsync(context, evt, isDiscovery);
					}
				}

				// Flush remaining
				if (lineBuffer.Length > 0)
				{
					var remaining = lineBuffer.ToString().TrimEnd('\r');
					if (!string.IsNullOrWhiteSpace(remaining))
					{
						var evt = TestResultEvent.Parse(remaining);
						if (evt is not null)
							await ProcessEventAsync(context, evt, isDiscovery);
					}
				}
			}
			catch (OperationCanceledException) when (!context.CancellationToken.IsCancellationRequested)
			{
				// Timeout — report as infrastructure failure
				var errorNode = new TestNode
				{
					Uid = new TestNodeUid("DeviceRunners.InfrastructureError"),
					DisplayName = "Device communication timeout",
					Properties = new PropertyBag(
						new FailedTestNodeStateProperty("The device app did not connect or stopped sending data within the timeout period.")),
				};
				await context.MessageBus.PublishAsync(
					this,
					new TestNodeUpdateMessage(context.Request.Session.SessionUid, errorNode));
			}
			finally
			{
				listener.Stop();
			}
		}
		finally
		{
			// MANDATORY — MTP/dotnet test hangs without this
			context.Complete();
		}
	}

	async Task ProcessEventAsync(ExecuteRequestContext context, TestResultEvent evt, bool isDiscovery)
	{
		// Skip begin/end control events — they don't map to test nodes
		if (evt.Type is TestResultEvent.TypeBegin or TestResultEvent.TypeEnd)
			return;

		var testNode = MapToTestNode(evt, isDiscovery);
		var parentUid = evt.ParentUid is not null ? new TestNodeUid(evt.ParentUid) : null;

		await context.MessageBus.PublishAsync(
			this,
			new TestNodeUpdateMessage(context.Request.Session.SessionUid, testNode, parentUid));
	}

	static TestNode MapToTestNode(TestResultEvent evt, bool isDiscovery)
	{
		TestNodeStateProperty stateProperty = evt.Type switch
		{
			TestResultEvent.TypeDiscovered => DiscoveredTestNodeStateProperty.CachedInstance,
			TestResultEvent.TypeInProgress => InProgressTestNodeStateProperty.CachedInstance,
			TestResultEvent.TypeResult when evt.Status == "Passed" => PassedTestNodeStateProperty.CachedInstance,
			TestResultEvent.TypeResult when evt.Status == "Failed" => new FailedTestNodeStateProperty(evt.ErrorMessage ?? string.Empty),
			TestResultEvent.TypeResult when evt.Status == "Skipped" => SkippedTestNodeStateProperty.CachedInstance,
			_ => isDiscovery
				? DiscoveredTestNodeStateProperty.CachedInstance
				: PassedTestNodeStateProperty.CachedInstance,
		};

		var properties = new PropertyBag(stateProperty);

		if (evt.Duration is not null && TimeSpan.TryParse(evt.Duration, out var duration))
		{
			var endTime = DateTimeOffset.UtcNow;
			var startTime = endTime - duration;
			properties.Add(new TimingProperty(new TimingInfo(startTime, endTime, duration)));
		}

		return new TestNode
		{
			Uid = new TestNodeUid(evt.Uid ?? evt.DisplayName ?? "unknown"),
			DisplayName = evt.DisplayName ?? "Unknown Test",
			Properties = properties,
		};
	}
}
