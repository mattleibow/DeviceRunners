namespace DeviceRunners.UIAutomation.Playwright;

public static partial class PlaywrightAutomatedAppOptionsBuilderExtensions
{
	public static TBuilder AddDefaultPlaywrightCommands<TBuilder>(this TBuilder builder)
		where TBuilder : PlaywrightAutomatedAppOptionsBuilder
	{
		builder.AddCommand(new PlaywrightGetPageSourceCommand());
		builder.AddCommand(new PlaywrightGetScreenshotCommand());
		builder.AddCommand(new PlaywrightGetElementTextCommand());
		builder.AddCommand(new PlaywrightClickElementCommand());
		builder.AddCommand(new PlaywrightClickCoordinatesCommand());

		return builder;
	}
}
