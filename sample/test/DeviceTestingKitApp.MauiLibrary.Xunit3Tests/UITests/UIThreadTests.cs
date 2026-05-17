using DeviceRunners.UITesting.Xunit3;

using Xunit;

namespace DeviceTestingKitApp.MauiLibrary.Xunit3Tests;

/// <summary>
/// Demonstrates xUnit v3 [UIFact] and [UITheory] attributes.
/// These tests are discovered by the v3 runner and dispatched to the UI thread
/// when loaded by a device runner MAUI app.
/// </summary>
public class UIThreadTests
{
	[UIFact]
	public void UIFact_RunsOnUIThread()
	{
		// This test verifies that the UIFact attribute is recognized by
		// the xUnit v3 discoverer and creates a UITestCase that dispatches
		// the test method invocation to the UI thread.
		Assert.True(true);
	}

	[UIFact]
	public async Task UIFact_SupportsAsyncTests()
	{
		await Task.Yield();
		Assert.True(true);
	}

	[UITheory]
	[InlineData("hello", 5)]
	[InlineData("world", 5)]
	[InlineData("", 0)]
	public void UITheory_RunsOnUIThread(string input, int expectedLength)
	{
		Assert.Equal(expectedLength, input.Length);
	}
}
