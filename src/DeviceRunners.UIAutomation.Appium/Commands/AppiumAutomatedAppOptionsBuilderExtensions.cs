namespace DeviceRunners.UIAutomation.Appium;

public static partial class AppiumAutomatedAppOptionsBuilderExtensions
{
	public static TBuilder AddDefaultAppiumCommands<TBuilder>(this TBuilder builder)
		where TBuilder : AppiumAutomatedAppOptionsBuilder
	{
		builder.AddCommand(new AppiumGetPageSourceCommand());
		builder.AddCommand(new AppiumGetScreenshotCommand());
		builder.AddCommand(new AppiumGetElementTextCommand());
		builder.AddCommand(new AppiumClickElementCommand());
		builder.AddCommand(new AppiumClickCoordinatesCommand());

		return builder;
	}
}
