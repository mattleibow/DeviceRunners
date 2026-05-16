using Microsoft.Extensions.DependencyInjection;

namespace DeviceTestingKitApp.BlazorLibrary.XunitTests.Controls;

/// <summary>
/// Base class for component tests — mirrors MAUI's VisualElementTests.
/// Sets up a service provider in constructor, tears down in Dispose.
/// Exercises the IDisposable fixture pattern.
/// </summary>
public class ComponentTestBase : IDisposable
{
	protected IServiceProvider Services { get; }

	public ComponentTestBase()
	{
		var sc = new ServiceCollection();
		ConfigureServices(sc);
		Services = sc.BuildServiceProvider();
	}

	protected virtual void ConfigureServices(IServiceCollection services)
	{
		services.AddLogging();
	}

	public void Dispose()
	{
		if (Services is IDisposable disposable)
			disposable.Dispose();
	}
}
