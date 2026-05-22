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

		// Merge registered resource dictionaries (order matters — Colors before Styles)
		var resourceOptions = Handler!.MauiContext!.Services.GetService<VisualRunnerResourceOptions>();
		if (resourceOptions is not null)
		{
			foreach (var factory in resourceOptions.ResourceDictionaryFactories)
			{
				Resources.MergedDictionaries.Add(factory());
			}
		}

		return Handler.MauiContext.Services.GetRequiredService<VisualRunnerWindow>();
	}
}
