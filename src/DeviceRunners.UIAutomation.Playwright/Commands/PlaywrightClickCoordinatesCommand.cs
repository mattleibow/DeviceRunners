namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightClickCoordinatesCommand : AutomatedAppCommand<PlaywrightAutomatedApp>
{
	public PlaywrightClickCoordinatesCommand()
		: base(CommonCommandNames.ClickCoordinates)
	{
	}

	public override object? Execute(PlaywrightAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		if (parameters is null)
			throw new ArgumentException("No coordinates found in parameters", nameof(parameters));
		if (!parameters.TryGetValue("x", out var x))
			throw new ArgumentException("X coordinate not found in parameters", nameof(parameters));
		if (!parameters.TryGetValue("y", out var y))
			throw new ArgumentException("Y coordinate not found in parameters", nameof(parameters));

		app.Page.Mouse.ClickAsync(Convert.ToInt32(x), Convert.ToInt32(y)).GetAwaiter().GetResult();

		return null;
	}
}
