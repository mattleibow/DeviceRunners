namespace DeviceTestingKitApp.MauiLibrary.XunitTests.Controls;

public class VisualElementTests : IDisposable
{
	public VisualElementTests()
	{
		Assert.Null(DispatcherProvider.Current.GetForCurrentThread());

		DispatcherProvider.SetCurrent(new DummyDispatcherProvider());
	}

	public void Dispose()
	{
		Assert.IsType<DummyDispatcher>(DispatcherProvider.Current.GetForCurrentThread());

		DispatcherProvider.SetCurrent(null);
	}

	class DummyDispatcherProvider : IDispatcherProvider
	{
		public IDispatcher? GetForCurrentThread() => new DummyDispatcher();
	}

	class DummyDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;

		public IDispatcherTimer CreateTimer() => throw new NotImplementedException();

		public bool Dispatch(Action action)
		{
			action();
			return true;
		}

		public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotImplementedException();
	}
}
