using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Xunit.Runner.Devices.XHarness.Maui;

public class HomeViewModel
{
	readonly XHarnessRunner _runner;

	public HomeViewModel(XHarnessRunner runner)
	{
		_runner = runner;
	}

	public Task RunTestsAsync() =>
		_runner.RunTestsAsync();
}
