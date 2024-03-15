using DeviceTestingKitApp.Controls;
using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.MauiLibrary.XunitTests.Controls;

public class CounterViewTests : VisualElementTests
{
	[Fact]
	public void InitialStateIsCorrect()
	{
		var counterView = new CounterView();
		var vm = new CounterViewModel();
		counterView.BindingContext = vm;

		var btn = Assert.IsType<Button>(counterView.Content);

		Assert.Equal("Click me!", btn.Text);
	}

	[Fact]
	public void InvokingCommandUpdatesButtonText()
	{
		var counterView = new CounterView();
		var vm = new CounterViewModel();
		counterView.BindingContext = vm;

		vm.IncrementCommand.Execute(null);

		var btn = Assert.IsType<Button>(counterView.Content);

		Assert.Equal("Clicked 1 time", btn.Text);
	}

	[Fact]
	public void FakeTappingButtonUpdatesButtonText()
	{
		var counterView = new CounterView();
		var vm = new CounterViewModel();
		counterView.BindingContext = vm;

		var btn = Assert.IsType<Button>(counterView.Content);
		
		// trigger a fake click using internal APIs
		btn.SendClicked();

		Assert.Equal("Clicked 1 time", btn.Text);
	}
}
