using NUnit.Framework;

namespace DeviceTestingKitApp.AppTests;

/// <summary>
/// Tests that verify the real app's pages can be instantiated within
/// the test runner, with styles and resources resolving correctly.
/// This demonstrates the "separate test app referencing the real app" pattern.
/// </summary>
[TestFixture]
public class MainPageTests
{
	[Test]
	public void MainPage_CanBeCreated()
	{
		// Arrange & Act — MainPage requires a MainViewModel
		var vm = new DeviceTestingKitApp.ViewModels.MainViewModel(
			new DeviceTestingKitApp.ViewModels.CounterViewModel());
		var page = new DeviceTestingKitApp.MainPage(vm);

		// Assert — page was created without {StaticResource} failures
		Assert.That(page, Is.Not.Null);
	}

	[Test]
	public void MainPage_HasCorrectBindingContext()
	{
		var vm = new DeviceTestingKitApp.ViewModels.MainViewModel(
			new DeviceTestingKitApp.ViewModels.CounterViewModel());
		var page = new DeviceTestingKitApp.MainPage(vm);

		Assert.That(page.BindingContext, Is.SameAs(vm));
	}

	[Test]
	public void AppStyles_AreAvailable()
	{
		// Verify that the app's styles are available in Application.Resources
		var app = Application.Current;
		Assert.That(app, Is.Not.Null);

		var hasHeadline = app!.Resources.TryGetValue("Headline", out var headlineStyle);
		Assert.That(hasHeadline, Is.True, "Headline style should be available from the app's Styles.xaml");
		Assert.That(headlineStyle, Is.InstanceOf<Style>());

		var hasSubHeadline = app.Resources.TryGetValue("SubHeadline", out var subHeadlineStyle);
		Assert.That(hasSubHeadline, Is.True, "SubHeadline style should be available from the app's Styles.xaml");
		Assert.That(subHeadlineStyle, Is.InstanceOf<Style>());
	}

	[Test]
	public void AppColors_AreAvailable()
	{
		var app = Application.Current;
		Assert.That(app, Is.Not.Null);

		var hasPrimary = app!.Resources.TryGetValue("Primary", out var primary);
		Assert.That(hasPrimary, Is.True, "Primary color should be available from the app's Colors.xaml");
		Assert.That(primary, Is.InstanceOf<Color>());
	}
}
