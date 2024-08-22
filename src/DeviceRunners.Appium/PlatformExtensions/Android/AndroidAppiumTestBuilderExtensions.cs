using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace DeviceRunners.Appium;

public static class AndroidAppiumTestBuilderExtensions
{
	public static AppiumTestBuilder AddAndroidApp(this AppiumTestBuilder builder, string appKey, string app, string? activityName = null) =>
		builder.AddAndroidApp(appKey, appBuilder =>
		{
			switch (Path.GetExtension(app)?.ToLowerInvariant())
			{
				case ".apk":
				case ".apks":
					appBuilder.UseAppPackageFilePath(app);
					break;
				case ".aab":
					throw new ArgumentException(
						"App packages with the .aab extension is not supported by Appium. Only .apk and .apks are supported.",
						nameof(app));
				default:
					appBuilder.UseAppPackageName(app, activityName ?? ".MainActivity");
					break;
			}
		});

	public static AppiumTestBuilder AddAndroidApp(this AppiumTestBuilder builder, string appKey, Action<AndroidAppiumTestAppBuilder> appBuilderAction) =>
		builder.AddApp<AndroidAppiumTestAppBuilder, DriverFactory>(appKey, appBuilderAction);

	class DriverFactory : IAppiumDriverFactory
	{
		public AppiumDriver CreateDriver(AppiumDriverManagerOptions options, AppiumServiceManager appium) =>
			new AndroidDriver(appium.Service.ServiceUrl, options.Options);
	}
}
