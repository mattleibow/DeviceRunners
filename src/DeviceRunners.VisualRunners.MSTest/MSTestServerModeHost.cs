using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;

using Microsoft.Testing.Platform.Builder;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeviceRunners.VisualRunners.MSTest;

/// <summary>
/// Hosts a Microsoft.Testing.Platform application in <b>server mode</b> in-process and drives it
/// over the platform's JSON-RPC protocol — the same mechanism Visual Studio and VS Code use.
/// <para>
/// The test application is the JSON-RPC <i>server</i>: it connects out over TCP to a client host
/// and port. Here we play the <i>client</i>: we listen on a loopback port, launch the platform in
/// the background, perform the <c>initialize</c> handshake, then issue a discovery or run request
/// and stream back the <c>testing/testUpdates/tests</c> node notifications. Unlike the console
/// host, server mode delivers node updates for a discovery request without executing any tests.
/// </para>
/// </summary>
static class MSTestServerModeHost
{
	public const string DiscoverTestsMethod = "testing/discoverTests";
	public const string RunTestsMethod = "testing/runTests";

	const int InitializeRequestId = 0;
	const int OperationRequestId = 1;

	static readonly Assembly ClientAssembly = typeof(MSTestServerModeHost).Assembly;
	static readonly string ClientName = ClientAssembly.GetName().Name!;
	static readonly string ClientVersion =
		ClientAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
		?? ClientAssembly.GetName().Version?.ToString()
		?? "0.0.0";

	public readonly record struct TestNodeRef(string Uid, string DisplayName);

	public static async Task RunSessionAsync(
		Assembly assembly,
		string method,
		IReadOnlyCollection<TestNodeRef>? tests,
		Action<WireTestNode> onNode,
		CancellationToken cancellationToken)
	{
		using var listener = new TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		try
		{
			var port = ((IPEndPoint)listener.LocalEndpoint).Port;

			// Launch the platform as a JSON-RPC server that connects back to our listener.
			var serverArgs = new[]
			{
				"--server", "jsonrpc",
				"--client-host", "127.0.0.1",
				"--client-port", port.ToString(),
			};

			var serverTask = Task.Run(() => RunServerAsync(assembly, serverArgs), cancellationToken);

			try
			{
				var acceptTask = listener.AcceptTcpClientAsync(cancellationToken).AsTask();

				// If the server faults before connecting, don't wait for a connection that will
				// never arrive — surface the server failure instead.
				if (await Task.WhenAny(acceptTask, serverTask) == serverTask)
				{
					await serverTask;
					throw new InvalidOperationException("The MSTest server-mode host exited before establishing a connection.");
				}

				using var client = await acceptTask;
				using var stream = client.GetStream();

				await DriveSessionAsync(stream, method, tests, onNode, cancellationToken);
			}
			finally
			{
				// Surface any server-side failure (and let it finish shutting down).
				await serverTask;
			}
		}
		finally
		{
			listener.Stop();
		}
	}

	static async Task RunServerAsync(Assembly assembly, string[] serverArgs)
	{
		// Server mode is selected by the '--server' option in the args; a dedicated builder factory
		// is no longer needed (and is obsolete).
		var builder = await TestApplication.CreateBuilderAsync(serverArgs);

		builder.AddMSTest(() => new[] { assembly });

		using var app = await builder.BuildAsync();

		await app.RunAsync();
	}

	static async Task DriveSessionAsync(
		Stream stream,
		string method,
		IReadOnlyCollection<TestNodeRef>? tests,
		Action<WireTestNode> onNode,
		CancellationToken cancellationToken)
	{
		// 1. initialize handshake.
		await SendAsync(stream, new
		{
			jsonrpc = "2.0",
			id = InitializeRequestId,
			method = "initialize",
			@params = new
			{
				processId = Environment.ProcessId,
				clientInfo = new { name = ClientName, version = ClientVersion },
				capabilities = new { testing = new { debuggerProvider = false } },
			},
		}, cancellationToken);

		await ReadUntilResponseAsync(stream, InitializeRequestId, onNode: null, cancellationToken);

		// 2. discovery or run request.
		object requestParams = tests is null
			? new { runId = Guid.NewGuid().ToString() }
			: new
			{
				runId = Guid.NewGuid().ToString(),
				tests = tests.Select(t => new Dictionary<string, object>
				{
					["uid"] = t.Uid,
					["display-name"] = t.DisplayName,
				}).ToArray(),
			};

		await SendAsync(stream, new
		{
			jsonrpc = "2.0",
			id = OperationRequestId,
			method,
			@params = requestParams,
		}, cancellationToken);

		// 3. stream node updates until the request completes.
		await ReadUntilResponseAsync(stream, OperationRequestId, onNode, cancellationToken);

		// 4. ask the server to exit.
		await SendAsync(stream, new { jsonrpc = "2.0", method = "exit" }, cancellationToken);
	}

	static async Task ReadUntilResponseAsync(Stream stream, int requestId, Action<WireTestNode>? onNode, CancellationToken cancellationToken)
	{
		while (true)
		{
			var body = await ReadFramedMessageAsync(stream, cancellationToken);
			if (body is null)
				return; // connection closed

			using var document = JsonDocument.Parse(body);
			var root = document.RootElement;

			// A response to our request carries a matching id and a result/error.
			if (root.TryGetProperty("id", out var idElement) &&
				idElement.ValueKind == JsonValueKind.Number &&
				idElement.GetInt32() == requestId &&
				(root.TryGetProperty("result", out _) || root.TryGetProperty("error", out _)))
			{
				return;
			}

			if (onNode is not null &&
				root.TryGetProperty("method", out var methodElement) &&
				methodElement.ValueKind == JsonValueKind.String &&
				methodElement.GetString() == "testing/testUpdates/tests")
			{
				DispatchNodeUpdates(root, onNode);
			}
		}
	}

	static void DispatchNodeUpdates(JsonElement root, Action<WireTestNode> onNode)
	{
		if (!root.TryGetProperty("params", out var parameters) ||
			!parameters.TryGetProperty("changes", out var changes) ||
			changes.ValueKind != JsonValueKind.Array)
		{
			return;
		}

		foreach (var change in changes.EnumerateArray())
		{
			if (change.TryGetProperty("node", out var node) && node.ValueKind == JsonValueKind.Object)
				onNode(WireTestNode.FromJson(node));
		}
	}

	static async Task SendAsync(Stream stream, object message, CancellationToken cancellationToken)
	{
		// Default encoder escapes non-ASCII to \uXXXX, so the UTF-8 byte count equals the character
		// count the platform's StreamReader.ReadBlockAsync expects from the Content-Length header.
		var body = JsonSerializer.SerializeToUtf8Bytes(message);
		var header = Encoding.ASCII.GetBytes(
			$"Content-Length: {body.Length}\r\nContent-Type: application/testingplatform\r\n\r\n");

		await stream.WriteAsync(header, cancellationToken);
		await stream.WriteAsync(body, cancellationToken);
		await stream.FlushAsync(cancellationToken);
	}

	static async Task<byte[]?> ReadFramedMessageAsync(Stream stream, CancellationToken cancellationToken)
	{
		var header = new List<byte>(64);
		var one = new byte[1];

		while (true)
		{
			var read = await stream.ReadAsync(one.AsMemory(0, 1), cancellationToken);
			if (read == 0)
				return header.Count == 0 ? null : throw new EndOfStreamException("Connection closed mid-message.");

			header.Add(one[0]);

			var count = header.Count;
			if (count >= 4 && header[count - 4] == '\r' && header[count - 3] == '\n' && header[count - 2] == '\r' && header[count - 1] == '\n')
				break;
		}

		var contentLength = ParseContentLength(Encoding.ASCII.GetString(header.ToArray()));
		if (contentLength < 0)
			throw new InvalidDataException("Missing Content-Length header in server-mode message.");

		var body = new byte[contentLength];
		await stream.ReadExactlyAsync(body, cancellationToken);
		return body;
	}

	static int ParseContentLength(string headers)
	{
		foreach (var line in headers.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
		{
			const string prefix = "Content-Length:";
			if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
				int.TryParse(line.AsSpan(prefix.Length).Trim(), out var length))
			{
				return length;
			}
		}

		return -1;
	}
}
