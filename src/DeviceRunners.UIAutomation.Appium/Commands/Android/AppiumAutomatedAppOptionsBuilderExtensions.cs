namespace DeviceRunners.UIAutomation.Appium;

public static partial class AppiumAutomatedAppOptionsBuilderExtensions
{
	public static TBuilder AddDefaultAndroidAppiumCommands<TBuilder>(this TBuilder builder)
		where TBuilder : AppiumAutomatedAppOptionsBuilder
	{
		builder.AddCommand(new AndroidAppiumDismissKeyboardCommand());

		return builder;
	}
}
