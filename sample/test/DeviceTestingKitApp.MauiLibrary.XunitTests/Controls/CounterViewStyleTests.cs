using DeviceTestingKitApp.Controls;
using DeviceTestingKitApp.ViewModels;

namespace DeviceTestingKitApp.MauiLibrary.XunitTests.Controls;

[Collection("VisualElements")]
public class CounterViewStyleTests : VisualElementTests
{
	public CounterViewStyleTests()
	{
		View = new CounterView();
		ViewModel = new CounterViewModel();
		View.BindingContext = ViewModel;
	}

	public CounterView View { get; }

	public CounterViewModel ViewModel { get; }

	[Fact]
	public void ButtonUsesDefaultColorsWhenNoResourceDictionary()
	{
		var btn = Assert.IsType<Button>(View.Content);

		// Without resources, DynamicResource properties remain at their defaults
		Assert.Equal(Button.BackgroundColorProperty.DefaultValue, btn.BackgroundColor);
		Assert.Equal(Button.TextColorProperty.DefaultValue, btn.TextColor);
	}

	[Fact]
	public void ButtonResolvesColorsFromCounterStyles()
	{
		var btn = Assert.IsType<Button>(View.Content);

		// Simulate what AddResourceDictionary does: merge the dictionary into resources
		// that the view can resolve from (use the view's own resources for unit testing)
		var styles = new CounterStyles();
		View.Resources.MergedDictionaries.Add(styles);

		// DynamicResource bindings resolve when resources become available
		Assert.Equal(Color.FromArgb("#512BD4"), btn.BackgroundColor);
		Assert.Equal(Colors.White, btn.TextColor);
	}
}

/// <summary>
/// Tests that the CounterStyles resource dictionary contains the expected resources.
/// Does not require visual element infrastructure.
/// </summary>
public class CounterStylesDictionaryTests
{
	[Fact]
	public void ContainsExpectedResources()
	{
		var styles = new CounterStyles();

		Assert.True(styles.ContainsKey("CounterButtonColor"));
		Assert.True(styles.ContainsKey("CounterButtonTextColor"));
		Assert.Equal(Color.FromArgb("#512BD4"), styles["CounterButtonColor"]);
		Assert.Equal(Colors.White, styles["CounterButtonTextColor"]);
	}
}
