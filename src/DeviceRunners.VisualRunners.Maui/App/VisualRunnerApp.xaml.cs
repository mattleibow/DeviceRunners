namespace DeviceRunners.VisualRunners.Maui;

partial class VisualRunnerApp : Application
{
	public VisualRunnerApp()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		if (Windows.Any(w => w is VisualRunnerWindow))
			throw new InvalidOperationException("Only a single instance of the test runner window is supported.");

		return Handler!.MauiContext!.Services.GetRequiredService<VisualRunnerWindow>();
	}
}
