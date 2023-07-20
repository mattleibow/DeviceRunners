namespace DeviceRunners.UITesting;

public static class UIThreadCoordinator
{
	public static IUIThreadCoordinator? Current { get; set; }

	public static Task<T> DispatchAsync<T>(Func<Task<T>> operation)
	{
		var coordinator = Current ?? throw new InvalidOperationException("Unable to run tests on a UI thread because no coordinator was provided. Set UIThreadCoordinator.Current to an instance of a coordinator.");
		return coordinator.DispatchAsync(operation);
	}
}
