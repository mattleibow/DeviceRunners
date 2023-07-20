using System.Diagnostics;

namespace DeviceRunners.VisualRunners;

public static class AsyncUtils
{
	public static async Task<T> RunAsync<T>(this Func<T> action)
	{
		var task = Task.Factory.StartNew(
			action,
			CancellationToken.None,
			TaskCreationOptions.LongRunning,
			TaskScheduler.Default);

		try
		{
			return await task.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);

			if (Debugger.IsAttached)
				Debugger.Break();

			throw;
		}
	}

	public static async Task RunAsync(this Action action)
	{
		var task = Task.Factory.StartNew(
			action,
			CancellationToken.None,
			TaskCreationOptions.LongRunning,
			TaskScheduler.Default);

		try
		{
			await task.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex);

			if (Debugger.IsAttached)
				Debugger.Break();

			throw;
		}
	}
}
