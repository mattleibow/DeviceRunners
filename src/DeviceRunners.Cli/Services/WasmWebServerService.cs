using System.Net;

namespace DeviceRunners.Cli.Services;

/// <summary>
/// A lightweight HTTP static file server for serving published WASM bundles.
/// Uses HttpListener — no ASP.NET Core dependency required.
/// </summary>
public class WasmWebServerService : IAsyncDisposable
{
	HttpListener? _listener;
	CancellationTokenSource? _cts;
	Task? _serverTask;
	string? _rootPath;

	static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
	{
		[".html"] = "text/html",
		[".htm"] = "text/html",
		[".js"] = "application/javascript",
		[".mjs"] = "application/javascript",
		[".cjs"] = "text/javascript",
		[".css"] = "text/css",
		[".json"] = "application/json",
		[".wasm"] = "application/wasm",
		[".dll"] = "application/octet-stream",
		[".pdb"] = "application/octet-stream",
		[".dat"] = "application/octet-stream",
		[".blat"] = "application/octet-stream",
		[".webcil"] = "application/octet-stream",
		[".png"] = "image/png",
		[".jpg"] = "image/jpeg",
		[".gif"] = "image/gif",
		[".svg"] = "image/svg+xml",
		[".ico"] = "image/x-icon",
		[".woff"] = "font/woff",
		[".woff2"] = "font/woff2",
		[".ttf"] = "font/ttf",
	};

	public string? BaseUrl { get; private set; }

	public Task StartAsync(string appPath, int port = 0)
	{
		_rootPath = Path.GetFullPath(appPath);
		if (!Directory.Exists(_rootPath))
			throw new DirectoryNotFoundException($"WASM app directory not found: {_rootPath}");

		// HttpListener doesn't support port 0 auto-assign directly,
		// so find an available port first
		if (port == 0)
			port = FindAvailablePort();

		var prefix = $"http://127.0.0.1:{port}/";

		_listener = new HttpListener();
		_listener.Prefixes.Add(prefix);
		_listener.Start();

		BaseUrl = $"http://127.0.0.1:{port}";

		_cts = new CancellationTokenSource();
		_serverTask = Task.Run(() => ServeRequestsAsync(_cts.Token));

		return Task.CompletedTask;
	}

	async Task ServeRequestsAsync(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested && _listener?.IsListening == true)
		{
			try
			{
				var context = await _listener.GetContextAsync().WaitAsync(ct);
				_ = Task.Run(() => HandleRequestAsync(context), ct);
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (HttpListenerException) when (ct.IsCancellationRequested)
			{
				break;
			}
			catch
			{
				// Log and continue
			}
		}
	}

	Task HandleRequestAsync(HttpListenerContext context)
	{
		try
		{
			var requestPath = Uri.UnescapeDataString(context.Request.Url?.AbsolutePath ?? "/");
			if (requestPath == "/")
				requestPath = "/index.html";

			var filePath = Path.Combine(_rootPath!, requestPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
			filePath = Path.GetFullPath(filePath);

			// Security: ensure the path is within the root
			if (!filePath.StartsWith(_rootPath!, StringComparison.Ordinal))
			{
				context.Response.StatusCode = 403;
				context.Response.Close();
				return Task.CompletedTask;
			}

			if (!File.Exists(filePath))
			{
				context.Response.StatusCode = 404;
				context.Response.Close();
				return Task.CompletedTask;
			}

			var ext = Path.GetExtension(filePath);
			var contentType = MimeTypes.GetValueOrDefault(ext, "application/octet-stream");

			context.Response.ContentType = contentType;
			context.Response.StatusCode = 200;

			// Cross-origin isolation headers for SharedArrayBuffer support
			context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
			context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";

			using var fileStream = File.OpenRead(filePath);
			fileStream.CopyTo(context.Response.OutputStream);
			context.Response.Close();
		}
		catch
		{
			try { context.Response.StatusCode = 500; context.Response.Close(); }
			catch { }
		}

		return Task.CompletedTask;
	}

	static int FindAvailablePort()
	{
		var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		var port = ((IPEndPoint)listener.LocalEndpoint).Port;
		listener.Stop();
		return port;
	}

	public async ValueTask DisposeAsync()
	{
		_cts?.Cancel();

		if (_listener?.IsListening == true)
		{
			_listener.Stop();
			_listener.Close();
		}

		if (_serverTask is not null)
		{
			try { await _serverTask; }
			catch { }
		}

		_cts?.Dispose();
		_listener = null;
	}
}
