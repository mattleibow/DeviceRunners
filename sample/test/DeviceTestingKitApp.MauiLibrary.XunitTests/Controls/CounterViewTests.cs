using DeviceTestingKitApp.Controls;
using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.MauiLibrary.XunitTests.Controls;

public class CounterViewTests : VisualElementTests
{
	public CounterViewTests()
	{
		View = new CounterView();
		ViewModel = new CounterViewModel();
		
		View.BindingContext = ViewModel;
	}

	public CounterView View { get; }

	public CounterViewModel ViewModel { get; }

	[Fact]
	public void InitialStateIsCorrect()
	{
		var btn = Assert.IsType<Button>(View.Content);

		Assert.Equal("Click me!", btn.Text);
	}

	[Fact]
	public void InvokingCommandUpdatesButtonText()
	{
		ViewModel.IncrementCommand.Execute(null);

		var btn = Assert.IsType<Button>(View.Content);

		Assert.Equal("Clicked 1 time", btn.Text);
	}

	[Fact]
	public void FakeTappingButtonUpdatesButtonText()
	{
		var btn = Assert.IsType<Button>(View.Content);
		
		// trigger a fake click using internal APIs
		btn.SendClicked();

		Assert.Equal("Clicked 1 time", btn.Text);
	}
}
