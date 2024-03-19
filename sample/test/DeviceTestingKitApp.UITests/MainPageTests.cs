using Xunit;
using Xunit.Abstractions;

namespace DeviceTestingKitApp.UITests;

public class MainPageTests : BaseUITests
{
	public MainPageTests(UITestsFixture fixture, ITestOutputHelper output)
		: base(fixture, output)
	{
	}

	[Fact]
	public void InitialStateIsCorrect()
	{
		var element = Driver.FindElement(DeviceBy.AutomationId("CounterButton"));

		Assert.Equal("Click me!", element.Text);
	}

	[Fact]
	public async Task SingleIncrementIncrementsByOne()
	{
		var element = Driver.FindElement(DeviceBy.AutomationId("CounterButton"));

		element.Click();
		await Task.Delay(500);

		Assert.Equal("Clicked 1 time", element.Text);
	}

	[Theory]
	[InlineData(0, "Click me!")]
	[InlineData(1, "Clicked 1 time")]
	[InlineData(2, "Clicked 2 times")]
	[InlineData(3, "Clicked 3 times")]
	public async Task ClickingMultipleTimesKeepsIncrementing(int clicks, string text)
	{
		var element = Driver.FindElement(DeviceBy.AutomationId("CounterButton"));

		for (var i = 0; i < clicks; i++)
		{
			element.Click();
			await Task.Delay(500);
		}

		Assert.Equal(text, element.Text);
	}
}
