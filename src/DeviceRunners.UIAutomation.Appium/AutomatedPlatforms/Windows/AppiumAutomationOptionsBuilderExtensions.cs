namespace DeviceRunners.UIAutomation.Appium;

public static partial class AppiumAutomationOptionsBuilderExtensions
{
	public static AppiumAutomationOptionsBuilder AddWindowsApp(this AppiumAutomationOptionsBuilder builder, string key, Action<WindowsAppiumAutomatedAppOptionsBuilder> optionsAction)
	{
		var optionsBuilder = new WindowsAppiumAutomatedAppOptionsBuilder(key);

		optionsBuilder.AddDefaultAppiumCommands();
		optionsBuilder.AddDefaultWindowsAppiumCommands();

		optionsAction(optionsBuilder);

		builder.AddApp(key, optionsBuilder.Build());

		return builder;
	}
}
