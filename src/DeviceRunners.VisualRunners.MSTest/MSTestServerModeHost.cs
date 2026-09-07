using System.Reflection;

using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.ServerMode.Client;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeviceRunners.VisualRunners.MSTest;

/// <summary>
/// Hosts a Microsoft.Testing.Platform application in <b>server mode</b> in-process and drives it
/// over the platform's JSON-RPC protocol — the same mechanism Visual Studio and VS Code use.
/// <para>
/// The protocol itself (transport, framing, handshake, serialization) comes from the platform's own
/// <c>Microsoft.Testing.Platform.ServerMode.Client.Sources</c> package: <see cref="MtpServerClient"/>
/// owns the loopback listener, launches the application through the callback below, and streams the
/// <c>testing/testUpdates/tests</c> node notifications back as events. Unlike the console host,
/// server mode delivers node updates for a discovery request without executing any tests.
/// </para>
/// </summary>
static class MSTestServerModeHost
{
	static readonly Assembly ClientAssembly = typeof(MSTestServerModeHost).Assembly;
	static readonly string ClientName = ClientAssembly.GetName().Name!;
	static readonly string ClientVersion =
		ClientAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
		?? ClientAssembly.GetName().Version?.ToString()
		?? "0.0.0";

	/// <summary>
	/// Discovers every test in <paramref name="assembly"/> without executing any of them.
	/// </summary>
	public static Task DiscoverTestsAsync(
		Assembly assembly,
		Action<MtpTestNodeUpdate> onNode,
		CancellationToken cancellationToken) =>
		RunSessionAsync(assembly, (client, token) => client.DiscoverTestsAsync(token), onNode, cancellationToken);

	/// <summary>
	/// Runs exactly the tests identified by <paramref name="testNodeUids"/>.
	/// </summary>
	public static Task RunTestsAsync(
		Assembly assembly,
		IReadOnlyCollection<string> testNodeUids,
		Action<MtpTestNodeUpdate> onNode,
		CancellationToken cancellationToken) =>
		RunSessionAsync(assembly, (client, token) => client.RunTestsAsync(testNodeUids, token), onNode, cancellationToken);

	static async Task RunSessionAsync(
		Assembly assembly,
		Func<IMtpServerClient, CancellationToken, Task> operation,
		Action<MtpTestNodeUpdate> onNode,
		CancellationToken cancellationToken)
	{
		var options = new MtpServerClientOptions
		{
			ClientName = ClientName,
			ClientVersion = ClientVersion,
		};

		using var client = await MtpServerClient.LaunchInProcessAsync(
			(serverArgs, token) => RunServerAsync(assembly, serverArgs),
			options,
			cancellationToken);

		// The read loop only starts on the first client operation, so subscribing here is enough to
		// see every update. Awaiting the operation guarantees all of its handlers have already run.
		client.TestNodesUpdated += OnTestNodesUpdated;

		try
		{
			await client.InitializeAsync(cancellationToken);

			await operation(client, cancellationToken);

			// Ask the server to exit before teardown closes the transport.
			await client.ExitAsync(cancellationToken);
		}
		finally
		{
			client.TestNodesUpdated -= OnTestNodesUpdated;

			// Tear the hosted application down asynchronously: the synchronous Dispose below then
			// returns immediately, which keeps the UI thread clear of the platform watchdogs
			// (Android ANR, the iOS watchdog).
			await client.ShutdownAsync();
		}

		void OnTestNodesUpdated(object? sender, MtpTestNodeUpdateEventArgs args)
		{
			foreach (var change in args.Changes)
				onNode(change);
		}
	}

	static async Task<int> RunServerAsync(Assembly assembly, string[] serverArgs)
	{
		// The client supplies the complete server-mode argument array ('--server jsonrpc' plus the
		// loopback client host/port); it must be forwarded verbatim.
		var builder = await TestApplication.CreateBuilderAsync(serverArgs);

		builder.AddMSTest(() => new[] { assembly });

		using var app = await builder.BuildAsync();

		return await app.RunAsync();
	}
}
