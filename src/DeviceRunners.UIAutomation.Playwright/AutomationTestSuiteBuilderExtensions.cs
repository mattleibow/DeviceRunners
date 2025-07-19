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
			new CompositeDiagnosticLogger(optionsBuilder.Loggers));

		builder.AddAutomationFramework(playwright);

		return builder;
	}
}
