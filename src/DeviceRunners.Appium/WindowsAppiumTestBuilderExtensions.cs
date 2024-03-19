using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DeviceRunners.Appium;

public static class WindowsAppiumTestBuilderExtensions
{
	public static AppiumTestBuilder AddWindowsApp(this AppiumTestBuilder builder, string appKey, string app) =>
		builder.AddApp(appKey, new AppiumDriverManagerOptions
		{
			Options = new AppiumOptions
			{
				AutomationName = "windows",
				PlatformName = "Windows",
				DeviceName = "WindowsPC",
				App = app,
			},
			DriverFactory = new WindowsDriverFactory(),
		});

	class WindowsDriverFactory : IAppiumDriverFactory
	{
		public AppiumDriver CreateDriver(AppiumDriverManagerOptions options, AppiumServiceManager appium) =>
			new WindowsDriver(appium.Service.ServiceUrl, options.Options);
	}
}
