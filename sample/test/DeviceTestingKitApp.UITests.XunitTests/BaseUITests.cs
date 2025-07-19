using System.Runtime.CompilerServices;
using System.Xml.Linq;

using DeviceRunners.UIAutomation;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DeviceTestingKitApp.UITests.XunitTests;

[Collection(UITestsCollection.CollectionName)]
public abstract class BaseUITests : IDisposable
{
	private readonly AutomationTestSuite _automationTestSuite;
	private readonly ITest? _xunitTest;

	public BaseUITests(UITestsFixture fixture, ITestOutputHelper output)
	{
		_automationTestSuite = fixture.TestSuite;
		App = _automationTestSuite.StartApp(_Config.Current);
		Output = output;

		_xunitTest = GetTest(Output as TestOutputHelper);
	}

	protected IAutomatedApp App { get; }

	protected ITestOutputHelper Output { get; }

	public void Dispose()
	{
		if (App?.GetPageSource() is string source && !string.IsNullOrWhiteSpace(source))
			Output.WriteLine($"Last page source:{Environment.NewLine}{FormatXml(source)}");
		else
			Output.WriteLine("Page source is empty");

		if (App?.GetScreenshot() is { } screenshot)
			Output.WriteLine($"Last screenshot:{Environment.NewLine}{screenshot.ToBase64String()}");
		else
			Output.WriteLine("No screenshot available");

		_automationTestSuite.StopApp(_Config.Current);
	}

	private static string FormatXml(string maybeXml)
	{
		try
		{
			return XDocument.Parse(maybeXml).ToString(SaveOptions.None);
		}
		catch
		{
			return maybeXml;
		}
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "test")]
	static extern ref ITest GetTest(TestOutputHelper? output);
}
