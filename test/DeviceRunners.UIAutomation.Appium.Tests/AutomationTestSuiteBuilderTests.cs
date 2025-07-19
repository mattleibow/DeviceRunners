using DeviceRunners.UIAutomation;
using DeviceRunners.UIAutomation.Appium;

using NSubstitute;

using Xunit;

namespace UIAutomationAppiumTests;

public class AutomationTestSuiteBuilderTests : IDisposable
{
	AutomationTestSuite? suite;

	[Fact]
	public void CanBuild()
	{
		suite = AutomationTestSuiteBuilder.Create()
			.AddAppium(options => { })
			.Build();

		Assert.NotNull(suite);
	}

	[Fact]
	public void CanAddFramework()
	{
		suite = AutomationTestSuiteBuilder.Create()
			.AddAppium(options => { })
			.Build();

		var framework = Assert.Single(suite.Frameworks);
		Assert.IsType<AppiumAutomationFramework>(framework);
	}

	[Fact]
	public void AppiumDefaultsAreCorrect()
	{
		suite = AutomationTestSuiteBuilder.Create()
			.AddAppium(options => { })
			.Build();

		var framework = Assert.Single(suite.Frameworks);
		var appium = Assert.IsType<AppiumAutomationFramework>(framework);
		Assert.Equal("http://127.0.0.1:14723/", appium.ServiceManager.Service.ServiceUrl.AbsoluteUri);
	}

	public void Dispose()
	{
		suite?.Dispose();
	}
}
