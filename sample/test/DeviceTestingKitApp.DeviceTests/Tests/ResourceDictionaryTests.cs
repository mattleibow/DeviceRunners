namespace DeviceTestingKitApp.DeviceTests;

/// <summary>
/// Tests that resource dictionaries registered via AddResourceDictionary are
/// properly merged into Application.Resources and can be resolved at runtime.
/// </summary>
public class ResourceDictionaryTests
{
	[Fact]
	public void RegisteredResourceDictionary_ResolvesStaticResource()
	{
		var app = Application.Current;
		Assert.NotNull(app);

		// "Primary" is defined in TestColors.xaml which is registered via AddResourceDictionary<TestColors>()
		var found = app!.Resources.TryGetValue("Primary", out var value);
		Assert.True(found, "Expected 'Primary' resource to be resolvable from Application.Resources");
		Assert.IsType<Color>(value);
	}

	[Fact]
	public void RegisteredResourceDictionary_ResolvesCustomColor()
	{
		var app = Application.Current;
		Assert.NotNull(app);

		// "TestAccent" is defined in TestColors.xaml
		var found = app!.Resources.TryGetValue("TestAccent", out var value);
		Assert.True(found, "Expected 'TestAccent' resource to be resolvable from Application.Resources");

		var color = Assert.IsType<Color>(value);
		Assert.Equal(Color.FromArgb("#FF5733"), color);
	}

	[Fact]
	public void RegisteredResourceDictionary_MergedIntoMergedDictionaries()
	{
		var app = Application.Current;
		Assert.NotNull(app);

		// The resource dictionaries registered via AddResourceDictionary should appear
		// in MergedDictionaries
		Assert.NotEmpty(app!.Resources.MergedDictionaries);
	}
}
