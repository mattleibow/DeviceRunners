using DeviceRunners.UIAutomation;
using DeviceRunners.UIAutomation.Appium;
using DeviceRunners.UIAutomation.Selenium;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceTestingKitApp.UITests.XunitTests;

public class UITestsFixture : IDisposable
{
	public UITestsFixture(IMessageSink diagnosticMessageSink)
	{
		var builder = AutomationTestSuiteBuilder.Create()
			.AddAppium(options => options
				.UseServiceAddress("127.0.0.1", 4723)
				.AddLogger(new MessageSinkLogger(diagnosticMessageSink))
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

	public void Dispose()
	{
		TestSuite.Dispose();
	}

	public AutomationTestSuite TestSuite { get; private set; }

	class MessageSinkLogger : IDiagnosticLogger
	{
		private readonly IMessageSink _diagnosticMessageSink;

		public MessageSinkLogger(IMessageSink diagnosticMessageSink) =>
			_diagnosticMessageSink = diagnosticMessageSink;

		public void Log(string message) =>
			_diagnosticMessageSink.OnMessage(new DiagnosticMessage(message));
	}
}

[CollectionDefinition(CollectionName)]
public class UITestsCollection : ICollectionFixture<UITestsFixture>
{
	public const string CollectionName = "Appium UI Tests";
}
