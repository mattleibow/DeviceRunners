﻿namespace DeviceRunners.UIAutomation.Appium;

public class AppiumGetPageSourceCommand : AutomatedAppCommand<AppiumAutomatedApp>
{
	public AppiumGetPageSourceCommand()
		: base(AppiumCommandNames.GetPageSource)
	{
	}

	public override object? Execute(AppiumAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		return app.Driver.PageSource;
	}
}
