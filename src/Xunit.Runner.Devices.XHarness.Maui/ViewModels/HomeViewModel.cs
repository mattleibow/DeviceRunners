using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Xunit.Runner.Devices.XHarness.Maui;

public class HomeViewModel
{
	readonly ITestRunner _runner;

	public HomeViewModel(ITestRunner runner)
	{
		_runner = runner;
	}

	public Task RunTestsAsync() =>
		_runner.RunTestsAsync();
}
