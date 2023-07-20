namespace DeviceRunners.XHarness.Maui;

partial class XHarnessApp : Application
{
	public XHarnessApp()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		if (Windows.Any(w => w is XHarnessWindow))
			throw new InvalidOperationException("Only a single instance of the XHarness window is supported.");

		return Handler!.MauiContext!.Services.GetRequiredService<XHarnessWindow>();
	}
}
