namespace DeviceTestingKitApp.MauiLibrary.XunitTests;

[Collection("UITests")]
public abstract class UITests<T> : IAsyncLifetime
	where T : Page
{
	protected T CurrentPage { get; private set; } = null!;

	protected IMauiContext MauiContext { get; private set; } = null!;

	public async Task InitializeAsync()
	{
		Routing.RegisterRoute("uitests", typeof(T));

		await Shell.Current.GoToAsync("uitests");

		CurrentPage = (T)Shell.Current.CurrentPage;

		await WaitForLoaded(CurrentPage);
		await WaitForPageLayout(CurrentPage);

		MauiContext = CurrentPage.Handler!.MauiContext!;
	}

	protected static async Task WaitForLoaded(VisualElement element, int timeout = 1000)
	{
		if (element.IsLoaded)
			return;

		var tcs = new TaskCompletionSource();

		element.Loaded += OnLoaded;

		await Task.WhenAny(tcs.Task, Task.Delay(timeout));

		element.Loaded -= OnLoaded;

		Assert.True(element.IsLoaded);

		void OnLoaded(object? sender, EventArgs e)
		{
			element.Loaded -= OnLoaded;
			tcs.SetResult();
		}
	}

	/// <summary>
	/// Waits for the page's platform view to be laid out with non-zero dimensions.
	/// On Android, Shell navigation can leave the CoordinatorLayout at 0x0 until
	/// a layout pass is explicitly triggered or the message loop processes layout requests.
	/// </summary>
	static async Task WaitForPageLayout(Page page, int timeout = 2000, int interval = 50)
	{
		if (page.Width > 0 && page.Height > 0)
			return;

		var elapsed = 0;
		while (elapsed < timeout)
		{
			await Task.Delay(interval);
			elapsed += interval;

			if (page.Width > 0 && page.Height > 0)
				return;
		}

#if ANDROID
		// Force a layout pass if the platform view tree hasn't been measured yet
		if (page.Handler?.PlatformView is Android.Views.View platformView && !platformView.IsLaidOut)
		{
			platformView.RootView?.RequestLayout();
			elapsed = 0;
			while (elapsed < timeout)
			{
				await Task.Delay(interval);
				elapsed += interval;

				if (page.Width > 0 && page.Height > 0)
					return;
			}
		}
#endif

		Assert.True(page.Width > 0 && page.Height > 0,
			$"Page was not laid out within timeout. Width={page.Width}, Height={page.Height}");
	}

	public async Task DisposeAsync()
	{
		CurrentPage = null!;

		await Shell.Current.GoToAsync("..");

		Routing.UnRegisterRoute("uitests");
	}
}
