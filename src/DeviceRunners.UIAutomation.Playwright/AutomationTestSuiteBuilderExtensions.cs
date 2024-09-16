namespace DeviceRunners.UIAutomation.Playwright;

public static class AutomationTestSuiteBuilderExtensions
{
	public static AutomationTestSuiteBuilder AddPlaywright(this AutomationTestSuiteBuilder builder, Action<PlaywrightAutomationOptionsBuilder> optionsAction)
	{
		var optionsBuilder = new PlaywrightAutomationOptionsBuilder();

		optionsAction(optionsBuilder);

		var playwright = new PlaywrightAutomationFramework(
			optionsBuilder.Options,
			optionsBuilder.Apps,
			new CompositeLogger(optionsBuilder.Loggers));

		builder.AddAutomationFramework(playwright);

		return builder;
	}

	class CompositeLogger : IPlaywrightDiagnosticLogger
	{
		private readonly List<IPlaywrightDiagnosticLogger> _loggers;

		public CompositeLogger(IEnumerable<IPlaywrightDiagnosticLogger> loggers) =>
			_loggers = loggers.ToList();

		public void Log(string message)
		{
			foreach (var logger in _loggers)
				logger.Log(message);
		}
	}
}
