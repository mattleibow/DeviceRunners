using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DeviceRunners.VisualRunners;

public class AsyncLock
{
	readonly SemaphoreSlim _semaphore;
	readonly Task<IDisposable> _releaser;

	public AsyncLock()
	{
		_semaphore = new SemaphoreSlim(1);
		_releaser = Task.FromResult<IDisposable>(new Releaser(this));
	}

#if DEBUG
	public Task<IDisposable> LockAsync([CallerMemberName] string callingMethod = null!, [CallerFilePath] string path = null!, [CallerLineNumber] int line = 0)
	{
		Debug.WriteLine("AsyncLock.LockAsync called by: " + callingMethod + " in file: " + path + " : " + line);
#else
	public Task<IDisposable> LockAsync()
	{
#endif
		var wait = _semaphore.WaitAsync();

		return wait.IsCompleted
			? _releaser
			: wait.ContinueWith(
				static (_, state) => (IDisposable)new Releaser((AsyncLock)state!),
				this,
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);
	}

	readonly struct Releaser : IDisposable
	{
		readonly AsyncLock _toRelease;

		public Releaser(AsyncLock toRelease) =>
			_toRelease = toRelease;

		public void Dispose() =>
			_toRelease._semaphore.Release();
	}
}
