namespace DeviceRunners.UITesting.Xunit;

static class UIThreadAccessor
{
	// TODO: make this more complex to handle different windows and
	//       if/when they have different threads

	public static Task<T> DispatchAsync<T>(Func<Task<T>> operation)
	{
		var app = Application.Current ?? throw new InvalidOperationException("Unable to ran tests on the application's UI thred because there is no current application.");
		return app.Dispatcher.DispatchAsync(operation);
	}

}
