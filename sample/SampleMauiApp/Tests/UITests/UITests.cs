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
		MauiContext = CurrentPage.Handler!.MauiContext!;
		if (CurrentPage.IsLoaded)
			return;

		var tcs = new TaskCompletionSource();
		CurrentPage.Loaded += OnLoaded;

		await Task.WhenAny(tcs.Task, Task.Delay(1000));

		CurrentPage.Loaded -= OnLoaded;

		Assert.True(CurrentPage.IsLoaded);

		void OnLoaded(object? sender, EventArgs e)
		{
			CurrentPage.Loaded -= OnLoaded;
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
