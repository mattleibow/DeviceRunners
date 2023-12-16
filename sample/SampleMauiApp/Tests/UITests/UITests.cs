namespace SampleMauiApp;

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

	public async Task DisposeAsync()
	{
		CurrentPage = null!;

		await Shell.Current.GoToAsync("..");

		Routing.UnRegisterRoute("uitests");
	}
}
