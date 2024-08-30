namespace DeviceRunners.UIAutomation.Appium;

public class AndroidAppiumDismissKeyboardCommand : AutomatedAppCommand<AppiumAutomatedApp>
{
	public AndroidAppiumDismissKeyboardCommand()
		: base(AppiumCommonCommandNames.DismissKeyboard)
	{
	}

	public override object? Execute(AppiumAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		if (app.Driver.IsKeyboardShown())
			app.Driver.HideKeyboard();

		return null;
	}
}
