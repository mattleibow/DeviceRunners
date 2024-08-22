namespace DeviceRunners.Appium;

public static class AppiumTestBuilderExtensions
{
	internal static AppiumTestBuilder AddApp<TAppiumTestAppBuilder, TAppiumDriverFactory>(this AppiumTestBuilder builder, string appKey, Action<TAppiumTestAppBuilder> appBuilderAction)
		where TAppiumTestAppBuilder : AppiumTestAppBuilder, new()
		where TAppiumDriverFactory : IAppiumDriverFactory, new()
	{
		var appBuilder = new TAppiumTestAppBuilder();

		appBuilderAction?.Invoke(appBuilder);

		return builder.AddApp(appKey, new AppiumDriverManagerOptions
		{
			Options = appBuilder.AppiumOptions,
			DriverFactory = new TAppiumDriverFactory(),
		});
	}
}
