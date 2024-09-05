﻿using System.Xml.Linq;

using DeviceRunners.UIAutomation;

using NUnit.Framework.Internal;

namespace DeviceTestingKitApp.UITests.NUnitTests;

[TestFixture("android")]
[TestFixture("windows")]
[TestFixture("web")]
[Parallelizable(ParallelScope.All)]
public abstract class BaseUITests
{
	private readonly string _testKey;

	private AutomationTestSuite _automationTestSuite;
	private IAutomatedApp _app;

	public BaseUITests(string testKey)
	{
		_testKey = testKey;
	}

	protected IAutomatedApp App => _app;

	protected TextWriter Output => TestContext.Out;

	[SetUp]
	public void SetUp()
	{
		_automationTestSuite = UITestsSetupFixture.TestSuite;
		_app = _automationTestSuite.StartApp(_testKey);
	}

	[TearDown]
	public void TearDown()
	{
		if (App?.GetPageSource() is string source && !string.IsNullOrWhiteSpace(source))
			Output.WriteLine($"Last page source:{Environment.NewLine}{source}");
		else
			Output.WriteLine("Page source is empty");

		if (App?.GetScreenshot() is { } screenshot)
			Output.WriteLine($"Last screenshot:{Environment.NewLine}{screenshot.ToBase64String()}");
		else
			Output.WriteLine("No screenshot available");

		_automationTestSuite.StopApp(_testKey);
	}
}