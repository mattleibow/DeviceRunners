namespace DeviceRunners.UIAutomation.Playwright;

public static partial class PlaywrightAutomationOptionsBuilderExtensions
{
	public static PlaywrightAutomationOptionsBuilder AddMicrosoftEdge(this PlaywrightAutomationOptionsBuilder builder, string key, Action<EdgePlaywrightAutomatedAppOptionsBuilder> optionsAction)
	{
		var optionsBuilder = new EdgePlaywrightAutomatedAppOptionsBuilder(key);

		optionsBuilder.AddDefaultPlaywrightCommands();

		optionsAction(optionsBuilder);

		builder.AddApp(key, optionsBuilder.Build());

		return builder;
	}
}
