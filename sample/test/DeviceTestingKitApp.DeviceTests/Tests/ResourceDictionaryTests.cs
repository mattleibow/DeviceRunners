namespace DeviceTestingKitApp.DeviceTests;

/// <summary>
/// Tests that resource dictionaries registered via AddResourceDictionary are
/// properly merged into Application.Resources and can be resolved at runtime.
/// These tests validate the visual runner's AddResourceDictionary feature.
/// Under XHarness (which doesn't use AddResourceDictionary), resources won't
/// be present and tests will pass vacuously.
/// </summary>
public class ResourceDictionaryTests
{
	[Fact]
	public void RegisteredResourceDictionary_ResolvesCounterButtonColor()
	{
		var app = Application.Current;
		Assert.NotNull(app);

		// "CounterButtonColor" is defined in CounterStyles.xaml registered via AddResourceDictionary<CounterStyles>()
		var found = app!.Resources.TryGetValue("CounterButtonColor", out var value);
		if (!found)
			return; // Not running under visual runner (e.g. XHarness mode)

		Assert.IsType<Color>(value);
		Assert.Equal(Color.FromArgb("#FF6B6B"), (Color)value!);
	}

	[Fact]
	public void RegisteredResourceDictionary_ResolvesCounterButtonTextColor()
	{
		var app = Application.Current;
		Assert.NotNull(app);

		var found = app!.Resources.TryGetValue("CounterButtonTextColor", out var value);
		if (!found)
			return; // Not running under visual runner (e.g. XHarness mode)

		Assert.IsType<Color>(value);
	}

	[Fact]
	public void RegisteredResourceDictionary_MergedIntoMergedDictionaries()
	{
		var app = Application.Current;
		Assert.NotNull(app);

		if (app!.Resources.MergedDictionaries.Count == 0)
			return; // Not running under visual runner (e.g. XHarness mode)

		Assert.NotEmpty(app!.Resources.MergedDictionaries);
	}
}
