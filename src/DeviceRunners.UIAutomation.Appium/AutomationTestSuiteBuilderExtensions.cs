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
			new CompositeDiagnosticLogger(optionsBuilder.Loggers));

		builder.AddAutomationFramework(appium);

		return builder;
	}
}
