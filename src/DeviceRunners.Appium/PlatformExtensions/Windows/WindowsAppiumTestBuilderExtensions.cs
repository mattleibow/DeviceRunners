using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DeviceRunners.Appium;

public static class WindowsAppiumTestBuilderExtensions
{
	public static AppiumTestBuilder AddWindowsApp(this AppiumTestBuilder builder, string appKey, string app) =>
		builder.AddWindowsApp(appKey, appBuilder => appBuilder.UseApp(app));

	public static AppiumTestBuilder AddWindowsApp(this AppiumTestBuilder builder, string appKey, Action<WindowsAppiumTestAppBuilder> appBuilderAction) =>
		builder.AddApp<WindowsAppiumTestAppBuilder, DriverFactory>(appKey, appBuilderAction);

	class DriverFactory : IAppiumDriverFactory
	{
		public AppiumDriver CreateDriver(AppiumDriverManagerOptions options, AppiumServiceManager appium) =>
			new WindowsDriver(appium.Service.ServiceUrl, options.Options);
	}
}
