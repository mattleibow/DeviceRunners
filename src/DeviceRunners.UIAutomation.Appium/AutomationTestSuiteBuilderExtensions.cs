namespace DeviceRunners.UIAutomation.Appium;

public static class AutomationTestSuiteBuilderExtensions
{
	public static AutomationTestSuiteBuilder AddAppium(this AutomationTestSuiteBuilder builder, Action<AppiumAutomationOptionsBuilder> optionsAction)
	{
		var optionsBuilder = new AppiumAutomationOptionsBuilder();

		optionsAction(optionsBuilder);

		var appium = new AppiumAutomationFramework(
			optionsBuilder.Options,
			optionsBuilder.Apps,
			new CompositeLogger(optionsBuilder.Loggers));

		builder.AddAutomationFramework(appium);

		return builder;
	}

	class CompositeLogger : IAppiumDiagnosticLogger
	{
		private readonly List<IAppiumDiagnosticLogger> _loggers;

		public CompositeLogger(IEnumerable<IAppiumDiagnosticLogger> loggers) =>
			_loggers = loggers.ToList();

		public void Log(string message)
		{
			foreach (var logger in _loggers)
				logger.Log(message);
		}
	}
}
