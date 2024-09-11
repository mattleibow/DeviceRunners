//namespace DeviceRunners.UIAutomation.Playwright;

//public static partial class PlaywrightAutomationOptionsBuilderExtensions
//{
//	public static PlaywrightAutomationOptionsBuilder AddAndroidApp(this PlaywrightAutomationOptionsBuilder builder, string key, Action<AndroidPlaywrightAutomatedAppOptionsBuilder> optionsAction)
//	{
//		var optionsBuilder = new AndroidPlaywrightAutomatedAppOptionsBuilder(key);

//		optionsBuilder.AddDefaultPlaywrightCommands();
//		optionsBuilder.AddDefaultAndroidPlaywrightCommands();

//		optionsAction(optionsBuilder);

//		builder.AddApp(key, optionsBuilder.Build());

//		return builder;
//	}
//}
