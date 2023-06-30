namespace Xunit.Runner.Devices.Maui;

partial class TestRunnerApp : Application
{
	public TestRunnerApp()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		if (Windows.Any(w => w is TestRunnerWindow))
			throw new InvalidOperationException("Only a single instance of the test runner window is supported.");

		return Handler!.MauiContext!.Services.GetRequiredService<TestRunnerWindow>();
	}
}
