namespace DeviceRunners.UIAutomation.Playwright;

public static partial class PlaywrightAutomatedAppOptionsBuilderExtensions
{
	public static TBuilder UseInitialUrl<TBuilder>(this TBuilder builder, string initialUrl)
		where TBuilder : PlaywrightAutomatedAppOptionsBuilder
	{
		builder.LaunchOptions[PlaywrightBrowserLaunchOptionKeys.InitialUrl] = initialUrl;
		return builder;
	}

	public static TBuilder ConfigureHeadless<TBuilder>(this TBuilder builder, bool headless)
		where TBuilder : PlaywrightAutomatedAppOptionsBuilder
	{
		var launchOptions = builder.LaunchOptions.GetOrAddBrowserTypeLaunchOptions();
		launchOptions.Headless = headless;

		return builder;
	}
}
