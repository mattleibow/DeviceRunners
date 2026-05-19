using Bunit;
using DeviceTestingKitApp.Controls;
using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.BlazorLibrary.XunitTests.Controls;

/// <summary>
/// Mirrors MAUI's CounterViewTests.FakeTappingButtonUpdatesButtonText —
/// dispatches an actual click event through the rendered component to
/// exercise the full UI event → command → re-render pipeline.
/// </summary>
public class CounterComponentClickTests : Bunit.TestContext
{
	[Fact]
	public void ClickingButtonUpdatesRenderedText()
	{
		var vm = new CounterViewModel();

		var cut = RenderComponent<CounterComponent>(p => p.Add(c => c.ViewModel, vm));

		Assert.Contains("Click me!", cut.Find("button").TextContent);

		cut.Find("button").Click();

		Assert.Equal(1, vm.Count);
		Assert.Contains("Clicked 1 time", cut.Find("button").TextContent);
	}

	[Fact]
	public void MultipleClicksAccumulate()
	{
		var vm = new CounterViewModel();

		var cut = RenderComponent<CounterComponent>(p => p.Add(c => c.ViewModel, vm));

		cut.Find("button").Click();
		cut.Find("button").Click();
		cut.Find("button").Click();

		Assert.Equal(3, vm.Count);
		Assert.Contains("Clicked 3 times", cut.Find("button").TextContent);
	}
}
