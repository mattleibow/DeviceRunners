namespace DeviceRunners.UIAutomation.Selenium;

public static class AutomationTestSuiteBuilderExtensions
{
	public static AutomationTestSuiteBuilder AddSelenium(this AutomationTestSuiteBuilder builder, Action<SeleniumAutomationOptionsBuilder> optionsAction)
	{
		var optionsBuilder = new SeleniumAutomationOptionsBuilder();

		optionsAction(optionsBuilder);

		var Selenium = new SeleniumAutomationFramework(
			optionsBuilder.Apps,
			new CompositeLogger(optionsBuilder.Loggers));

		builder.AddAutomationFramework(Selenium);

		return builder;
	}

	class CompositeLogger : ISeleniumDiagnosticLogger
	{
		private readonly List<ISeleniumDiagnosticLogger> _loggers;

		public CompositeLogger(IEnumerable<ISeleniumDiagnosticLogger> loggers) =>
			_loggers = loggers.ToList();

		public void Log(string message)
		{
			foreach (var logger in _loggers)
				logger.Log(message);
		}
	}
}
