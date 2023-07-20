namespace DeviceRunners.UITesting;

public interface IUIThreadCoordinator
{
	Task<T> DispatchAsync<T>(Func<Task<T>> operation);
}
