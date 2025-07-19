namespace DeviceRunners.UIAutomation;

public class AutomationTestSuiteBuilder
{
	private readonly List<IAutomationFramework> _automationFrameworks = [];

	private AutomationTestSuiteBuilder()
	{
	}

	public static AutomationTestSuiteBuilder Create() => new();

	public AutomationTestSuiteBuilder AddAutomationFramework(IAutomationFramework framework)
	{
		_automationFrameworks.Add(framework);

		return this;
	}

	public AutomationTestSuite Build()
	{
		var apps = new HashSet<string>();

		foreach (var framework in _automationFrameworks)
		{
			foreach (var app in framework.AvailableApps)
			{
				if (!apps.Add(app.Key))
				{
					throw new InvalidOperationException($"App with key '{app.Key}' is registered multiple times.");
				}
			}
		}

		return new AutomationTestSuite(_automationFrameworks);
	}
}
