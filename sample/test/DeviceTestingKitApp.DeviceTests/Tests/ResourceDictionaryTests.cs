namespace DeviceTestingKitApp.DeviceTests;

/// <summary>
/// Tests that resource dictionaries registered via AddResourceDictionary are
/// properly merged into Application.Resources and can be resolved at runtime.
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
		Assert.True(found, "Expected 'CounterButtonColor' resource to be resolvable from Application.Resources");
		Assert.IsType<Color>(value);
		Assert.Equal(Color.FromArgb("#FF6B6B"), (Color)value!);
	}

	[Fact]
	public void RegisteredResourceDictionary_ResolvesCounterButtonTextColor()
	{
		var app = Application.Current;
		Assert.NotNull(app);

		var found = app!.Resources.TryGetValue("CounterButtonTextColor", out var value);
		Assert.True(found, "Expected 'CounterButtonTextColor' resource to be resolvable from Application.Resources");
		Assert.IsType<Color>(value);
	}

	[Fact]
	public void RegisteredResourceDictionary_MergedIntoMergedDictionaries()
	{
		var app = Application.Current;
		Assert.NotNull(app);

		Assert.NotEmpty(app!.Resources.MergedDictionaries);
	}
}
