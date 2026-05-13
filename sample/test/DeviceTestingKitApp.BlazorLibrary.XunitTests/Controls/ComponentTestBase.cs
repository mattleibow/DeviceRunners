using Microsoft.Extensions.DependencyInjection;

namespace DeviceTestingKitApp.BlazorLibrary.XunitTests.Controls;

/// <summary>
/// Base class for component tests — mirrors MAUI's VisualElementTests.
/// Sets up a service provider in constructor, verifies and tears down in Dispose.
/// Exercises the IDisposable fixture pattern.
/// </summary>
public class ComponentTestBase : IDisposable
{
	static IServiceProvider? s_current;

	protected IServiceProvider Services { get; }

	public ComponentTestBase()
	{
		Assert.Null(s_current);

		var sc = new ServiceCollection();
		ConfigureServices(sc);
		Services = sc.BuildServiceProvider();

		s_current = Services;
	}

	protected virtual void ConfigureServices(IServiceCollection services)
	{
		services.AddLogging();
	}

	public void Dispose()
	{
		Assert.Same(Services, s_current);

		if (Services is IDisposable disposable)
			disposable.Dispose();

		s_current = null;
	}
}
