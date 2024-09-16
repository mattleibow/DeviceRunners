using DeviceRunners.UIAutomation;

namespace DeviceTestingKitApp.UITests.NUnitTests;

public class MainPageTests : BaseUITests
{
	public MainPageTests(string appKey)
		: base(appKey)
	{
	}

	[Test]
	public void InitialStateIsCorrect()
	{
		var element = App.FindElement(by => by.Id("CounterButton"));
		Assert.NotNull(element);

		Assert.That(element.GetText(), Is.EqualTo("Click me!"));
	}

	[Test]
	public async Task SingleIncrementIncrementsByOne()
	{
		var element = App.FindElement(by => by.Id("CounterButton"));
		Assert.NotNull(element);

		element.Click();
		await Task.Delay(500);

		Assert.That(element.GetText(), Is.EqualTo("Clicked 1 time"));
	}

	[Test]
	[TestCase(0, "Click me!")]
	[TestCase(1, "Clicked 1 time")]
	[TestCase(2, "Clicked 2 times")]
	[TestCase(3, "Clicked 3 times")]
	public async Task ClickingMultipleTimesKeepsIncrementing(int clicks, string text)
	{
		var element = App.FindElement(by => by.Id("CounterButton"));
		Assert.NotNull(element);

		for (var i = 0; i < clicks; i++)
		{
			element.Click();
			await Task.Delay(500);
		}

		Assert.That(element.GetText(), Is.EqualTo(text));
	}
}
