using OpenQA.Selenium.Interactions;

namespace DeviceRunners.UIAutomation.Selenium;

public class SeleniumClickCoordinatesCommand : AutomatedAppCommand<SeleniumAutomatedApp>
{
	public SeleniumClickCoordinatesCommand()
		: base(CommonCommandNames.ClickCoordinates)
	{
	}

	public override object? Execute(SeleniumAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null)
	{
		if (parameters is null)
			throw new ArgumentException("No coordinates found in parameters", nameof(parameters));
		if (!parameters.TryGetValue("x", out var x))
			throw new ArgumentException("X coordinate not found in parameters", nameof(parameters));
		if (!parameters.TryGetValue("y", out var y))
			throw new ArgumentException("Y coordinate not found in parameters", nameof(parameters));

		var touchDevice = new PointerInputDevice(PointerKind.Mouse);
		var sequence = new ActionSequence(touchDevice, 0);
		sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, Convert.ToInt32(x), Convert.ToInt32(y), TimeSpan.FromMilliseconds(5)));
		sequence.AddAction(touchDevice.CreatePointerDown(MouseButton.Left));
		sequence.AddAction(touchDevice.CreatePointerUp(MouseButton.Left));
		app.Driver.PerformActions([sequence]);

		return null;
	}
}
