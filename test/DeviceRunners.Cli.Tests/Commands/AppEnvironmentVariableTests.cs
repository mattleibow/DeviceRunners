using DeviceRunners.Cli.Commands;

namespace DeviceRunners.Cli.Tests;

public class AppEnvironmentVariableTests
{
	[Fact]
	public void Filter_WhenSet_IsAddedToEnvironment()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
			Filter = "FullyQualifiedName~MyClass",
		};

		var env = BaseTestCommand<WindowsTestCommand.Settings>.GetAppEnvironmentVariables(settings);

		Assert.Equal("FullyQualifiedName~MyClass", env["DEVICE_RUNNERS_FILTER"]);
		Assert.Equal("1", env["DEVICE_RUNNERS_AUTORUN"]);
	}

	[Fact]
	public void Filter_WhenNotSet_IsOmittedFromEnvironment()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
		};

		var env = BaseTestCommand<WindowsTestCommand.Settings>.GetAppEnvironmentVariables(settings);

		Assert.False(env.ContainsKey("DEVICE_RUNNERS_FILTER"));
	}

	[Fact]
	public void Filter_WhenWhitespace_IsOmittedFromEnvironment()
	{
		var settings = new WindowsTestCommand.Settings
		{
			App = "app.msix",
			Filter = "   ",
		};

		var env = BaseTestCommand<WindowsTestCommand.Settings>.GetAppEnvironmentVariables(settings);

		Assert.False(env.ContainsKey("DEVICE_RUNNERS_FILTER"));
	}
}
