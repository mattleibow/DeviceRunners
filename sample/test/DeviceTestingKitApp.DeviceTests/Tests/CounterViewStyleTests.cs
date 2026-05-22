using DeviceTestingKitApp.Controls;
using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.DeviceTests;

#if !MODE_XHARNESS
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
#endif
