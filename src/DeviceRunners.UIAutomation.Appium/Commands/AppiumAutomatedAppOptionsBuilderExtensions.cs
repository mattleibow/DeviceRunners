namespace DeviceRunners.UIAutomation.Appium;

public static partial class AppiumAutomatedAppOptionsBuilderExtensions
{
	public static TBuilder AddDefaultAppiumCommands<TBuilder>(this TBuilder builder)
		where TBuilder : AppiumAutomatedAppOptionsBuilder
	{
		builder.AddCommand(new AppiumGetPageSourceCommand());
		builder.AddCommand(new AppiumGetScreenshotCommand());

		return builder;
	}
}
