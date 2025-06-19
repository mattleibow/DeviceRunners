namespace DeviceRunners.UIAutomation.Selenium;

public static partial class SeleniumAutomatedAppOptionsBuilderExtensions
{
	public static TBuilder AddDefaultSeleniumCommands<TBuilder>(this TBuilder builder)
		where TBuilder : SeleniumAutomatedAppOptionsBuilder
	{
		builder.AddCommand(new SeleniumGetPageSourceCommand());
		builder.AddCommand(new SeleniumGetScreenshotCommand());
		builder.AddCommand(new SeleniumGetElementTextCommand());
		builder.AddCommand(new SeleniumClickElementCommand());
		builder.AddCommand(new SeleniumClickCoordinatesCommand());

		return builder;
	}
}
