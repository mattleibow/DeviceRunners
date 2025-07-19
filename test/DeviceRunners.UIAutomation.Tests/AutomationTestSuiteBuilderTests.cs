using DeviceRunners.UIAutomation;

using NSubstitute;

using Xunit;

namespace UIAutomationTests;

public class AutomationTestSuiteBuilderTests
{
	[Fact]
	public void DefaultsDoNotThrow()
	{
		var builder = AutomationTestSuiteBuilder.Create();

		Assert.NotNull(builder);
	}

	[Fact]
	public void CanBuild()
	{
		var builder = AutomationTestSuiteBuilder.Create();

		var suite = builder.Build();

		Assert.NotNull(suite);
	}

	[Fact]
	public void CanAddFramework()
	{
		var builder = AutomationTestSuiteBuilder.Create();

		var framework = Substitute.For<IAutomationFramework>();
		builder.AddAutomationFramework(framework);

		var suite = builder.Build();

		Assert.Equal([framework], suite.Frameworks);
	}

	[Fact]
	public void CanAddFrameworkAndApp()
	{
		var builder = AutomationTestSuiteBuilder.Create();

		var options = Substitute.For<IAutomatedAppOptions>();
		options.Key.Returns("test_app");

		var framework = Substitute.For<IAutomationFramework>();
		framework.AvailableApps.Returns([options]);

		builder.AddAutomationFramework(framework);

		var suite = builder.Build();

		Assert.Equal([framework], suite.Frameworks);
		Assert.Equal(["test_app"], suite.AvailableApps);
	}
}
