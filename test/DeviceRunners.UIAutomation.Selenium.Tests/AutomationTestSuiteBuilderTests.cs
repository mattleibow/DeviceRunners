using DeviceRunners.UIAutomation;
using DeviceRunners.UIAutomation.Selenium;

using Xunit;

namespace UIAutomationSeleniumTests;

public class AutomationTestSuiteBuilderTests : IDisposable
{
	AutomationTestSuite? suite;

	[Fact]
	public void CanBuild()
	{
		suite = AutomationTestSuiteBuilder.Create()
			.AddSelenium(options => { })
			.Build();

		Assert.NotNull(suite);
	}

	[Fact]
	public void CanAddFramework()
	{
		suite = AutomationTestSuiteBuilder.Create()
			.AddSelenium(options => { })
			.Build();

		var framework = Assert.Single(suite.Frameworks);
		Assert.IsType<SeleniumAutomationFramework>(framework);
	}

	public void Dispose()
	{
		suite?.Dispose();
	}
}
