namespace DeviceRunners.XHarness;

public class HomeViewModel
{
	readonly ITestRunner _runner;

	public HomeViewModel(ITestRunner runner)
	{
		_runner = runner;
	}

	public async Task RunTestsAsync()
	{
		var result = await _runner.RunTestsAsync();

		TestRunCompleted?.Invoke(this, result);
	}

	public event EventHandler<ITestRunResult>? TestRunCompleted;
}
