using Microsoft.Playwright;

namespace DeviceRunners.Cli.Services;

public class BrowserService : IAsyncDisposable
{
	IPlaywright? _playwright;
	IBrowser? _browser;
	IPage? _page;

	public event EventHandler<string>? ConsoleMessageReceived;

	public async Task LaunchAsync(string url, bool headless = true, string browserType = "chromium")
	{
		_playwright = await Playwright.CreateAsync();

		var browserTypeLauncher = browserType.ToLowerInvariant() switch
		{
			"chromium" or "chrome" => _playwright.Chromium,
			"firefox" => _playwright.Firefox,
			"webkit" or "safari" => _playwright.Webkit,
			_ => _playwright.Chromium
		};

		_browser = await browserTypeLauncher.LaunchAsync(new() { Headless = headless });
		_page = await _browser.NewPageAsync();

		_page.Console += (_, msg) =>
		{
			ConsoleMessageReceived?.Invoke(this, msg.Text);
		};

		await _page.GotoAsync(url, new() { WaitUntil = WaitUntilState.Load });
	}

	public async Task WaitForConsoleAsync(Func<string, bool> predicate, TimeSpan timeout)
	{
		var tcs = new TaskCompletionSource();
		using var cts = new CancellationTokenSource(timeout);

		cts.Token.Register(() => tcs.TrySetCanceled());

		void Handler(object? sender, string msg)
		{
			if (predicate(msg))
				tcs.TrySetResult();
		}

		ConsoleMessageReceived += Handler;
		try
		{
			await tcs.Task;
		}
		finally
		{
			ConsoleMessageReceived -= Handler;
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_page is not null)
		{
			await _page.CloseAsync();
			_page = null;
		}
		if (_browser is not null)
		{
			await _browser.CloseAsync();
			_browser = null;
		}
		_playwright?.Dispose();
		_playwright = null;
	}
}
