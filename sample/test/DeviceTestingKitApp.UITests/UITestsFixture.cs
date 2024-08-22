using DeviceRunners.Appium;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceTestingKitApp.UITests;

public class UITestsFixture : IDisposable
{
	public UITestsFixture(IMessageSink diagnosticMessageSink)
	{
		AppiumTest = AppiumTestBuilder.Create()
			.UseServiceAddress("127.0.0.1", 4723)
			.AddLogger(new MessageSinkLogger(diagnosticMessageSink))
			.AddWindowsApp("windows_msix", "com.companyname.devicetestingkitapp_9zz4h110yvjzm!App")
			.AddAndroidApp("android", "com.companyname.devicetestingkitapp", ".MainActivity")
			//.AddAndroidApp("android", "D:\\GitHub\\DeviceRunners\\sample\\src\\DeviceTestingKitApp\\bin\\Release\\net8.0-android\\com.companyname.devicetestingkitapp-Signed.apk")
			.Build();
	}

	public void Dispose()
	{
		AppiumTest.Dispose();
	}

	public AppiumTest AppiumTest { get; private set; }

	class MessageSinkLogger : IAppiumDiagnosticLogger
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
