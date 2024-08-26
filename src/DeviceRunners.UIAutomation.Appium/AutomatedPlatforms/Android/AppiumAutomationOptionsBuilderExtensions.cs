namespace DeviceRunners.UIAutomation.Appium;

public static partial class AppiumAutomationOptionsBuilderExtensions
{
	public static AppiumAutomationOptionsBuilder AddAndroidApp(this AppiumAutomationOptionsBuilder builder, string key, Action<AndroidAppiumAutomatedAppOptionsBuilder> optionsAction)
	{
		var optionsBuilder = new AndroidAppiumAutomatedAppOptionsBuilder(key);

		optionsBuilder.AddDefaultAppiumCommands();
		optionsBuilder.AddDefaultAndroidAppiumCommands();

		optionsAction(optionsBuilder);

		builder.AddApp(key, optionsBuilder.Build());

		return builder;
	}
}
