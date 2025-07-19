using DeviceRunners.UIAutomation;

using NSubstitute;

using Xunit;

namespace UIAutomationTests;

public class AutomationTestSuiteTests
{
	[Fact]
	public void DefaultsDoNotThrow()
	{
		var builder = AutomationTestSuiteBuilder.Create();
		var suite = builder.Build();

		Assert.NotNull(suite);
		Assert.Empty(suite.AvailableApps);
		Assert.Empty(suite.InstantiatedApps);
	}

	[Fact]
	public void RequestingInvalidAppThrows()
	{
		var framework = Substitute.For<IAutomationFramework>();
		var suite = BuildTestSuite(framework);

		Assert.Equal([framework], suite.Frameworks);
		Assert.Throws<KeyNotFoundException>(() => suite.GetApp("bad_test_app"));
	}

	[Fact]
	public void GetAppReturnsUnstartedAppWhenNotInstantiated()
	{
		var app = Substitute.For<IAutomatedApp>();
		var suite = BuildTestSuite(app: app);

		Assert.Equal(app, suite.GetApp("test_app"));
	}

	[Fact]
	public void StartAppReturnsAppAfterStarting()
	{
		var app = Substitute.For<IAutomatedApp>();
		var framework = Substitute.For<IAutomationFramework>();
		var suite = BuildTestSuite(framework, app);

		Assert.Equal(app, suite.StartApp("test_app"));
		Assert.Equal(app, suite.GetApp("test_app"));

		framework.Received().CreateApp(Arg.Any<IAutomatedAppOptions>());
		framework.Received().StartApp(Arg.Any<IAutomatedApp>());
		framework.DidNotReceive().RestartApp(Arg.Any<IAutomatedApp>());
		framework.DidNotReceive().StopApp(Arg.Any<IAutomatedApp>());
	}

	[Fact]
	public void RestartAppReturnsAppAfterStartingButNotActuallyRestarting()
	{
		var app = Substitute.For<IAutomatedApp>();
		var framework = Substitute.For<IAutomationFramework>();
		var suite = BuildTestSuite(framework, app);

		Assert.Equal(app, suite.RestartApp("test_app"));
		Assert.Equal(app, suite.GetApp("test_app"));

		framework.Received().CreateApp(Arg.Any<IAutomatedAppOptions>());
		framework.Received().StartApp(Arg.Any<IAutomatedApp>());
		framework.DidNotReceive().RestartApp(Arg.Any<IAutomatedApp>());
		framework.DidNotReceive().StopApp(Arg.Any<IAutomatedApp>());
	}

	private static AutomationTestSuite BuildTestSuite(IAutomationFramework? framework = null, IAutomatedApp? app = null)
	{
		var builder = AutomationTestSuiteBuilder.Create();

		var options = Substitute.For<IAutomatedAppOptions>();
		options.Key.Returns("test_app");

		app ??= Substitute.For<IAutomatedApp>();

		framework ??= Substitute.For<IAutomationFramework>();
		framework.AvailableApps.Returns([options]);
		framework.CreateApp(Arg.Is(options)).Returns(app);

		builder.AddAutomationFramework(framework);

		var suite = builder.Build();

		return suite;
	}
}
