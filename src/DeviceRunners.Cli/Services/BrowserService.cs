using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DeviceRunners.Cli.Services;

/// <summary>
/// Launches a system-installed Chrome/Chromium in headless mode and captures
/// console output via the Chrome DevTools Protocol (CDP) over WebSocket.
/// No external tools (Playwright, Selenium, npm) required.
/// </summary>
public class BrowserService : IAsyncDisposable
{
	Process? _chromeProcess;
	ClientWebSocket? _webSocket;
	CancellationTokenSource? _readCts;
	Task? _readTask;
	int _cdpId;
	string? _userDataDir;

	public event EventHandler<string>? ConsoleMessageReceived;

	public async Task LaunchAsync(string url, bool headless = true)
	{
		var chromePath = FindChrome();
		if (chromePath is null)
			throw new FileNotFoundException(
				"Could not find Chrome/Chromium. Install Chrome or set the CHROME_PATH environment variable.");

		// Launch Chrome with remote debugging on an auto-assigned port
		_userDataDir = Path.Combine(Path.GetTempPath(), "device-runners-chrome-" + Guid.NewGuid().ToString("N")[..8]);
		Directory.CreateDirectory(_userDataDir);

		var args = new List<string>
		{
			"--remote-debugging-port=0",
			$"--user-data-dir={_userDataDir}",
			"--no-first-run",
			"--no-default-browser-check",
			"--disable-extensions",
			"--disable-background-networking",
			"--disable-sync",
			"--disable-translate",
			"--metrics-recording-only",
			"--safebrowsing-disable-auto-update",
			"--disable-gpu",
		};

		if (headless)
			args.Add("--headless=new");

		args.Add("about:blank");

		_chromeProcess = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = chromePath,
				Arguments = string.Join(' ', args),
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			}
		};

		_chromeProcess.Start();

		// Chrome prints the DevTools URL to stderr: "DevTools listening on ws://..."
		var debugUrl = await ReadDevToolsUrlAsync(_chromeProcess, TimeSpan.FromSeconds(30));

		// Connect to the browser's CDP WebSocket
		var wsUrl = await GetPageWebSocketUrl(debugUrl);

		_webSocket = new ClientWebSocket();
		await _webSocket.ConnectAsync(new Uri(wsUrl), CancellationToken.None);

		// Enable Runtime events to capture console.log
		await SendCdpCommandAsync("Runtime.enable");

		// Start reading CDP messages in background
		_readCts = new CancellationTokenSource();
		_readTask = Task.Run(() => ReadCdpMessagesAsync(_readCts.Token));

		// Navigate to the test URL
		await SendCdpCommandAsync("Page.enable");
		await SendCdpCommandAsync("Page.navigate", new CdpNavigateParams { Url = url });
	}

	static async Task<string> ReadDevToolsUrlAsync(Process process, TimeSpan timeout)
	{
		using var cts = new CancellationTokenSource(timeout);
		var stderr = process.StandardError;
		var stderrLines = new List<string>();

		try
		{
			while (!cts.IsCancellationRequested)
			{
				var line = await stderr.ReadLineAsync(cts.Token);
				if (line is null)
					break;

				stderrLines.Add(line);

				// Chrome outputs: "DevTools listening on ws://127.0.0.1:PORT/devtools/browser/GUID"
				if (line.Contains("DevTools listening on"))
				{
					var wsUrl = line.Substring(line.IndexOf("ws://", StringComparison.Ordinal));
					// Extract the HTTP base URL from the WebSocket URL
					var uri = new Uri(wsUrl);
					return $"http://{uri.Host}:{uri.Port}";
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Timeout expired — fall through to build a descriptive error
		}

		var message = "Chrome did not output a DevTools URL within the timeout period.";

		if (process.HasExited)
			message += $" Process exited with code {process.ExitCode}.";

		if (stderrLines.Count > 0)
			message += $" Stderr output:{Environment.NewLine}{string.Join(Environment.NewLine, stderrLines)}";

		throw new TimeoutException(message);
	}

	static async Task<string> GetPageWebSocketUrl(string debugUrl)
	{
		using var http = new HttpClient();

		// Wait for Chrome to be ready and create a page
		for (int i = 0; i < 30; i++)
		{
			try
			{
				var pages = await http.GetFromJsonAsync(
					$"{debugUrl}/json",
					BrowserJsonContext.Default.ChromeDebugPageArray);

				if (pages is not null)
				{
					foreach (var page in pages)
					{
						if (page.Type == "page" && page.WebSocketDebuggerUrl is not null)
							return page.WebSocketDebuggerUrl;
					}
				}
			}
			catch
			{
				// Chrome may not be ready yet
			}

			await Task.Delay(100);
		}

		throw new TimeoutException("Could not find a Chrome page to connect to.");
	}

	async Task SendCdpCommandAsync(string method)
	{
		if (_webSocket is null || _webSocket.State != WebSocketState.Open)
			return;

		var id = Interlocked.Increment(ref _cdpId);
		var command = new CdpCommand { Id = id, Method = method };
		var msg = JsonSerializer.Serialize(command, BrowserJsonContext.Default.CdpCommand);
		await _webSocket.SendAsync(Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true, CancellationToken.None);
	}

	async Task SendCdpCommandAsync(string method, CdpNavigateParams parameters)
	{
		if (_webSocket is null || _webSocket.State != WebSocketState.Open)
			return;

		var id = Interlocked.Increment(ref _cdpId);
		var command = new CdpCommandWithParams<CdpNavigateParams> { Id = id, Method = method, Params = parameters };
		var msg = JsonSerializer.Serialize(command, BrowserJsonContext.Default.CdpCommandWithParamsCdpNavigateParams);
		await _webSocket.SendAsync(Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true, CancellationToken.None);
	}

	async Task ReadCdpMessagesAsync(CancellationToken ct)
	{
		var buffer = new byte[64 * 1024];
		var messageBuffer = new StringBuilder();

		try
		{
			while (!ct.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
			{
				WebSocketReceiveResult result;
				try
				{
					result = await _webSocket.ReceiveAsync(buffer, ct);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (WebSocketException)
				{
					break;
				}

				if (result.MessageType == WebSocketMessageType.Close)
					break;

				messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

				if (result.EndOfMessage)
				{
					ProcessCdpMessage(messageBuffer.ToString());
					messageBuffer.Clear();
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	void ProcessCdpMessage(string json)
	{
		try
		{
			var evt = JsonSerializer.Deserialize(json, BrowserJsonContext.Default.CdpEvent);
			if (evt is null || evt.Method != "Runtime.consoleAPICalled")
				return;

			var args = evt.Params?.Args;
			if (args is null)
				return;

			// Build the console message from all arguments
			var parts = new List<string>();
			foreach (var arg in args)
			{
				if (arg.Value is not null)
					parts.Add(arg.Value.ToString()!);
				else if (arg.Description is not null)
					parts.Add(arg.Description);
				else if (arg.UnserializableValue is not null)
					parts.Add(arg.UnserializableValue);
			}

			if (parts.Count > 0)
			{
				var message = string.Join(' ', parts);
				ConsoleMessageReceived?.Invoke(this, message);
			}
		}
		catch (JsonException)
		{
			// Ignore malformed CDP messages
		}
	}

	static string? FindChrome()
	{
		// Check environment variable first
		var envPath = Environment.GetEnvironmentVariable("CHROME_PATH");
		if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
			return envPath;

		// Platform-specific search
		if (OperatingSystem.IsMacOS())
		{
			string[] paths =
			[
				"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
				"/Applications/Chromium.app/Contents/MacOS/Chromium",
				"/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
			];
			return paths.FirstOrDefault(File.Exists);
		}

		if (OperatingSystem.IsLinux())
		{
			string[] names = ["google-chrome", "google-chrome-stable", "chromium", "chromium-browser", "microsoft-edge"];
			foreach (var name in names)
			{
				try
				{
					var proc = Process.Start(new ProcessStartInfo("which", name)
					{
						RedirectStandardOutput = true,
						UseShellExecute = false,
					});
					var path = proc?.StandardOutput.ReadToEnd().Trim();
					proc?.WaitForExit();
					if (!string.IsNullOrEmpty(path) && File.Exists(path))
						return path;
				}
				catch
				{
				}
			}
		}

		if (OperatingSystem.IsWindows())
		{
			string[] paths =
			[
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe"),
			];
			return paths.FirstOrDefault(File.Exists);
		}

		return null;
	}

	public async ValueTask DisposeAsync()
	{
		_readCts?.Cancel();

		if (_readTask is not null)
		{
			try { await _readTask; }
			catch { /* already cancelled */ }
		}

		if (_webSocket is not null)
		{
			if (_webSocket.State == WebSocketState.Open)
			{
				try { await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); }
				catch { }
			}
			_webSocket.Dispose();
			_webSocket = null;
		}

		if (_chromeProcess is not null && !_chromeProcess.HasExited)
		{
			try
			{
				_chromeProcess.Kill(entireProcessTree: true);
				await _chromeProcess.WaitForExitAsync();
			}
			catch { }
			_chromeProcess.Dispose();
			_chromeProcess = null;
		}

		if (_userDataDir is not null)
		{
			try { Directory.Delete(_userDataDir, recursive: true); }
			catch { }
			_userDataDir = null;
		}

		_readCts?.Dispose();
	}
}
