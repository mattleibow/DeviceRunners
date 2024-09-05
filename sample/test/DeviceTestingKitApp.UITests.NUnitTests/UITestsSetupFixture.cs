﻿using DeviceRunners.UIAutomation;
using DeviceRunners.UIAutomation.Appium;
using DeviceRunners.UIAutomation.Selenium;

using NUnit.Framework.Internal;

namespace DeviceTestingKitApp.UITests.NUnitTests;

[SetUpFixture]
public class UITestsSetupFixture
{
	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		var builder = AutomationTestSuiteBuilder.Create()
			.AddAppium(appium => appium
				.UseServiceAddress("127.0.0.1", 4723)
				.AddLogger(new TestContextLogger())
				.AddAndroidApp("android", options => options
					.UsePackageName("com.companyname.devicetestingkitapp")
					.UseActivityName(".MainActivity"))
				.AddWindowsApp("windows", options => options
					.UseAppId("com.companyname.devicetestingkitapp_9zz4h110yvjzm!App")))
			.AddSelenium(selenium => selenium
				.AddMicrosoftEdge("web", options => options
					.UseInitialUrl("https://localhost:7096/")));

		TestSuite = builder.Build();
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		TestSuite.Dispose();
	}

	public static AutomationTestSuite TestSuite { get; private set; }

	class TestContextLogger : IAppiumDiagnosticLogger
	{
		public void Log(string message) =>
			TestContext.Out.WriteLine(message);
	}
}