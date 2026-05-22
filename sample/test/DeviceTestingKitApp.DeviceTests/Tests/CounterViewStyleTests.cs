using DeviceTestingKitApp.Controls;
using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.DeviceTests;

/// <summary>
/// Tests that CounterStyles registered via AddResourceDictionary are resolved
/// by the CounterView button at runtime through Application.Resources.
/// </summary>
public class CounterViewStyleTests
{
	[Fact]
	public void CounterButtonResolvesBackgroundColorFromRegisteredDictionary()
	{
		var view = new CounterView();
		view.BindingContext = new CounterViewModel();

		var btn = Assert.IsType<Button>(view.Content);

		// CounterStyles is registered via AddResourceDictionary<CounterStyles>() in MauiProgram.
		// DynamicResource on the button resolves CounterButtonColor from Application.Resources.
		Assert.Equal(Color.FromArgb("#FF6B6B"), btn.BackgroundColor);
	}

	[Fact]
	public void CounterButtonResolvesTextColorFromRegisteredDictionary()
	{
		var view = new CounterView();
		view.BindingContext = new CounterViewModel();

		var btn = Assert.IsType<Button>(view.Content);

		Assert.Equal(Colors.White, btn.TextColor);
	}


}
