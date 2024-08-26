namespace DeviceRunners.UIAutomation;

public abstract class AutomatedAppCommand<TAutomatedApp> : IAutomatedAppCommand
	where TAutomatedApp : IAutomatedApp
{
	public AutomatedAppCommand(string name)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
	}

	public string Name { get; }

	object? IAutomatedAppCommand.Execute(IAutomatedApp app, IReadOnlyDictionary<string, object>? parameters)
	{
		if (app is not TAutomatedApp appiumApp)
			throw new ArgumentException($"App must be an instance of {typeof(TAutomatedApp).Name} but was {app.GetType().Name}.", nameof(app));

		return Execute(appiumApp, parameters);
	}

	public abstract object? Execute(TAutomatedApp app, IReadOnlyDictionary<string, object>? parameters = null);
}
