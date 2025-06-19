namespace DeviceRunners.UIAutomation.Appium;

public static partial class AppiumAutomationOptionsBuilderExtensions
{
	public static AppiumAutomationOptionsBuilder AddMacApp(this AppiumAutomationOptionsBuilder builder, string key, Action<MacAppiumAutomatedAppOptionsBuilder> optionsAction)
	{
		var optionsBuilder = new MacAppiumAutomatedAppOptionsBuilder(key);

		optionsBuilder.AddDefaultAppiumCommands();

		optionsAction(optionsBuilder);

		builder.AddApp(key, optionsBuilder.Build());

		return builder;
	}
}
