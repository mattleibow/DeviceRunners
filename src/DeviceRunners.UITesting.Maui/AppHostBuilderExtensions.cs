using System.Diagnostics;

using DeviceRunners.UITesting.Maui;

namespace DeviceRunners.UITesting;

public static class AppHostBuilderExtensions
{
	public static MauiAppBuilder ConfigureUITesting(this MauiAppBuilder appHostBuilder)
	{
		var oldCurrent = UIThreadCoordinator.Current;

		if (oldCurrent is not MauiUIThreadCoordinator)
		{
			if (oldCurrent is not null)
			{
				var msg = $"UIThreadCoordinator already had a coordinator set and will override: '{oldCurrent}'";
				Debug.WriteLine(msg);
				Console.WriteLine(msg);
			}

			UIThreadCoordinator.Current = new MauiUIThreadCoordinator();
		}

		return appHostBuilder;
	}
}
