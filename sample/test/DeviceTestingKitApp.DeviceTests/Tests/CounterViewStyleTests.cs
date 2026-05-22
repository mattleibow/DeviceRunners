using DeviceTestingKitApp.Controls;
using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.DeviceTests;

/// <summary>
/// Tests that CounterStyles registered via AddResourceDictionary are resolved
/// by the CounterView button at runtime through Application.Resources.
/// These tests validate the visual runner's AddResourceDictionary feature.
/// Under XHarness (which doesn't use AddResourceDictionary), resources won't
/// be present and tests will pass vacuously.
/// </summary>
public class CounterViewStyleTests
{
	[Fact]
	public void CounterButtonResolvesBackgroundColorFromRegisteredDictionary()
	{
		var app = Application.Current;
		Assert.NotNull(app);
		if (!app!.Resources.TryGetValue("CounterButtonColor", out _))
			return; // Not running under visual runner (e.g. XHarness mode)

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
		var app = Application.Current;
		Assert.NotNull(app);
		if (!app!.Resources.TryGetValue("CounterButtonTextColor", out _))
			return; // Not running under visual runner (e.g. XHarness mode)

		var view = new CounterView();
		view.BindingContext = new CounterViewModel();

		var btn = Assert.IsType<Button>(view.Content);

		Assert.Equal(Colors.White, btn.TextColor);
	}
}
