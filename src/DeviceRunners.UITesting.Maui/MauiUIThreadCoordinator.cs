namespace DeviceRunners.UITesting.Maui;

public class MauiUIThreadCoordinator : IUIThreadCoordinator
{
	// TODO: make this more complex to handle different windows and
	//       if/when they have different threads

	public Task<T> DispatchAsync<T>(Func<Task<T>> operation)
	{
		var app = Application.Current ?? throw new InvalidOperationException("Unable to run tests on the application's UI thread because there is no current application.");
		return app.Dispatcher.DispatchAsync(operation);
	}
}
