namespace DeviceRunners.UIAutomation.Selenium;

public static partial class SeleniumAutomationOptionsBuilderExtensions
{
	public static SeleniumAutomationOptionsBuilder AddMicrosoftEdge(this SeleniumAutomationOptionsBuilder builder, string key, Action<EdgeSeleniumAutomatedAppOptionsBuilder> optionsAction)
	{
		var optionsBuilder = new EdgeSeleniumAutomatedAppOptionsBuilder(key);

		optionsBuilder.AddDefaultSeleniumCommands();
		//optionsBuilder.AddDefaultMicrosoftEdgeSeleniumCommands();

		optionsAction(optionsBuilder);

		builder.AddApp(key, optionsBuilder.Build());

		return builder;
	}
}
