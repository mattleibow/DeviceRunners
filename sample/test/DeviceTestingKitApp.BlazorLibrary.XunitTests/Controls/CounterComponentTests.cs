using DeviceTestingKitApp.Controls;
using DeviceTestingKitApp.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeviceTestingKitApp.BlazorLibrary.XunitTests.Controls;

public class CounterComponentTests : ComponentTestBase, IAsyncDisposable
{
	readonly HtmlRenderer _renderer;

	public CounterComponentTests()
	{
		_renderer = new HtmlRenderer(Services, Services.GetRequiredService<ILoggerFactory>());
	}

	public async ValueTask DisposeAsync()
	{
		await _renderer.DisposeAsync();
	}

	protected override void ConfigureServices(IServiceCollection services)
	{
		services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
		services.AddLogging();
	}

	[Fact]
	public async Task InitialStateIsCorrect()
	{
		var vm = new CounterViewModel();

		var html = await _renderer.Dispatcher.InvokeAsync(async () =>
		{
			var output = await _renderer.RenderComponentAsync<CounterComponent>(
				ParameterView.FromDictionary(new Dictionary<string, object?>
				{
					["ViewModel"] = vm
				}));
			return output.ToHtmlString();
		});

		Assert.Contains("Click me!", html);
	}

	[Fact]
	public async Task InvokingCommandUpdatesRenderedText()
	{
		var vm = new CounterViewModel();
		vm.IncrementCommand.Execute(null);

		var html = await _renderer.Dispatcher.InvokeAsync(async () =>
		{
			var output = await _renderer.RenderComponentAsync<CounterComponent>(
				ParameterView.FromDictionary(new Dictionary<string, object?>
				{
					["ViewModel"] = vm
				}));
			return output.ToHtmlString();
		});

		Assert.Contains("Clicked 1 time", html);
	}
}
