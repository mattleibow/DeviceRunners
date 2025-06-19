namespace DeviceRunners.UIAutomation.Selenium;

public static class AutomationTestSuiteBuilderExtensions
{
	public static AutomationTestSuiteBuilder AddSelenium(this AutomationTestSuiteBuilder builder, Action<SeleniumAutomationOptionsBuilder> optionsAction)
	{
		var optionsBuilder = new SeleniumAutomationOptionsBuilder();

		optionsAction(optionsBuilder);

		var Selenium = new SeleniumAutomationFramework(
			optionsBuilder.Apps,
			new CompositeDiagnosticLogger(optionsBuilder.Loggers));

		builder.AddAutomationFramework(Selenium);

		return builder;
	}
}
