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
		var vm = new DeviceTestingKitApp.ViewModels.MainViewModel(
			new DeviceTestingKitApp.ViewModels.CounterViewModel());
		var page = new DeviceTestingKitApp.MainPage(vm);

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

	[Test]
	public void MainPage_ContainsCounterButton()
	{
		var vm = new DeviceTestingKitApp.ViewModels.MainViewModel(
			new DeviceTestingKitApp.ViewModels.CounterViewModel());
		var page = new DeviceTestingKitApp.MainPage(vm);

		// Find the button by AutomationId in the visual tree
		var button = FindByAutomationId<Button>(page, "CounterButton");
		Assert.That(button, Is.Not.Null, "CounterButton should exist in the MainPage visual tree");
	}

	[Test]
	public void CounterButton_InitialText_IsClickMe()
	{
		var vm = new DeviceTestingKitApp.ViewModels.MainViewModel(
			new DeviceTestingKitApp.ViewModels.CounterViewModel());
		var page = new DeviceTestingKitApp.MainPage(vm);

		var button = FindByAutomationId<Button>(page, "CounterButton");
		Assert.That(button, Is.Not.Null);
		Assert.That(button!.Text, Is.EqualTo("Click me!"));
	}

	[Test]
	public void CounterButton_AfterOneClick_ViewModelUpdates()
	{
		var counter = new DeviceTestingKitApp.ViewModels.CounterViewModel();
		var vm = new DeviceTestingKitApp.ViewModels.MainViewModel(counter);
		var page = new DeviceTestingKitApp.MainPage(vm);

		var button = FindByAutomationId<Button>(page, "CounterButton");
		Assert.That(button, Is.Not.Null);

		// Execute the command (simulates button tap)
		counter.IncrementCommand.Execute(null);

		// ViewModel state should update
		Assert.That(counter.Count, Is.EqualTo(1));

		// Verify the binding is wired to the command
		Assert.That(button!.Command, Is.Not.Null);
		Assert.That(button.Command, Is.SameAs(counter.IncrementCommand));
	}

	[Test]
	public void CounterButton_AfterMultipleClicks_ViewModelTracksCount()
	{
		var counter = new DeviceTestingKitApp.ViewModels.CounterViewModel();
		var vm = new DeviceTestingKitApp.ViewModels.MainViewModel(counter);
		var page = new DeviceTestingKitApp.MainPage(vm);

		var button = FindByAutomationId<Button>(page, "CounterButton");
		Assert.That(button, Is.Not.Null);

		// Tap via the button's command binding
		button!.Command.Execute(null);
		button.Command.Execute(null);
		button.Command.Execute(null);

		Assert.That(counter.Count, Is.EqualTo(3));
	}

	[Test]
	public void CounterViewModel_IncrementCommand_CanExecute()
	{
		var counter = new DeviceTestingKitApp.ViewModels.CounterViewModel();

		Assert.That(counter.IncrementCommand.CanExecute(null), Is.True);
		Assert.That(counter.Count, Is.EqualTo(0));

		counter.IncrementCommand.Execute(null);
		Assert.That(counter.Count, Is.EqualTo(1));

		counter.IncrementCommand.Execute(null);
		Assert.That(counter.Count, Is.EqualTo(2));
	}

	[Test]
	public async Task CounterButton_WhenPageIsLive_BindingUpdatesText()
	{
		await MainThread.InvokeOnMainThreadAsync(async () =>
		{
			var counter = new DeviceTestingKitApp.ViewModels.CounterViewModel();
			var vm = new DeviceTestingKitApp.ViewModels.MainViewModel(counter);
			var page = new DeviceTestingKitApp.MainPage(vm);

			// Push as modal so the page gets a live handler and binding engine runs
			var currentPage = Application.Current!.Windows[0].Page!;
			await currentPage.Navigation.PushModalAsync(page);

			try
			{
				// Allow bindings to settle
				await Task.Delay(200);

				var button = FindByAutomationId<Button>(page, "CounterButton");
				Assert.That(button, Is.Not.Null);
				Assert.That(button!.Text, Is.EqualTo("Click me!"));

				// Tap the button via its command
				button.Command.Execute(null);
				await Task.Delay(200);

				Assert.That(counter.Count, Is.EqualTo(1));
				Assert.That(button.Text, Is.EqualTo("Clicked 1 time"));

				// Tap again
				button.Command.Execute(null);
				await Task.Delay(200);

				Assert.That(counter.Count, Is.EqualTo(2));
				Assert.That(button.Text, Is.EqualTo("Clicked 2 times"));
			}
			finally
			{
				await currentPage.Navigation.PopModalAsync();
			}
		});
	}

	static T? FindByAutomationId<T>(Element root, string automationId) where T : Element
	{
		if (root is T match && match.AutomationId == automationId)
			return match;

		IEnumerable<Element> children = root switch
		{
			IContentView { Content: Element content } => [content],
			Layout layout => layout.Children.OfType<Element>(),
			ContentPage { Content: View content } => [content],
			_ => []
		};

		foreach (var child in children)
		{
			var result = FindByAutomationId<T>(child, automationId);
			if (result is not null)
				return result;
		}

		return null;
	}
}
