using DeviceRunners.UIAutomation;
using DeviceRunners.UIAutomation.Playwright;

using Xunit;

namespace UIAutomationPlaywrightTests;

public class AutomationTestSuiteBuilderTests : IDisposable
{
	AutomationTestSuite? suite;

	[Fact]
	public void CanBuild()
	{
		suite = AutomationTestSuiteBuilder.Create()
			.AddPlaywright(options => { })
			.Build();

		Assert.NotNull(suite);
	}

	[Fact]
	public void CanAddFramework()
	{
		suite = AutomationTestSuiteBuilder.Create()
			.AddPlaywright(options => { })
			.Build();

		var framework = Assert.Single(suite.Frameworks);
		Assert.IsType<PlaywrightAutomationFramework>(framework);
	}

	public void Dispose()
	{
		suite?.Dispose();
	}
}
