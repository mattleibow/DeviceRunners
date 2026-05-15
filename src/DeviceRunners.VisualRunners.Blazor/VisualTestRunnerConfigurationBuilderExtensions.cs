namespace DeviceRunners.VisualRunners;

/// <summary>
/// Blazor-specific CLI configuration extensions for the visual test runner.
/// </summary>
public static class VisualTestRunnerConfigurationBuilderExtensions
{
	/// <summary>
	/// Configures the test runner from a URL containing query parameters.
	/// Parses the provided URL for <c>device-runners-autorun</c> query parameter.
	/// When the CLI launches the browser, it navigates to a URL with
	/// <c>?device-runners-autorun=1</c> to trigger headless mode with NDJSON
	/// console output via <see cref="ConsoleResultChannel"/>. When the parameter
	/// is absent (manual browser open), this is a no-op and the interactive
	/// visual runner is shown.
	/// </summary>
	public static TBuilder AddCliConfiguration<TBuilder>(this TBuilder builder, string currentUrl)
		where TBuilder : IVisualTestRunnerConfigurationBuilder
	{
		string? autorun = null;

		try
		{
			var qIdx = currentUrl.IndexOf('?');
			if (qIdx >= 0)
			{
				var query = currentUrl[(qIdx + 1)..];
				foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
				{
					var eqIdx = pair.IndexOf('=');
					var key = eqIdx >= 0 ? Uri.UnescapeDataString(pair[..eqIdx]) : pair;
					var value = eqIdx >= 0 ? Uri.UnescapeDataString(pair[(eqIdx + 1)..]) : "1";

					if (key.Equals("device-runners-autorun", StringComparison.OrdinalIgnoreCase))
						autorun = value;
				}
			}
		}
		catch
		{
			// Not a valid URL — ignore
		}

		if (string.IsNullOrEmpty(autorun))
			return builder;

		builder.EnableAutoStart(autoTerminate: true);
		builder.AddResultChannel(_ => new ConsoleResultChannel(new EventStreamFormatter()));
		return builder;
	}
}
