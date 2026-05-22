namespace DeviceRunners.VisualRunners.Maui;

partial class VisualRunnerApp : Application
{
	bool _resourcesMerged;

	public VisualRunnerApp()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		if (Windows.Any(w => w is VisualRunnerWindow))
			throw new InvalidOperationException("Only a single instance of the test runner window is supported.");

		// Merge registered resource dictionaries exactly once (order matters — Colors before Styles)
		if (!_resourcesMerged)
		{
			_resourcesMerged = true;

			var resourceOptions = Handler!.MauiContext!.Services.GetService<VisualRunnerResourceOptions>();
			if (resourceOptions is not null)
			{
				foreach (var factory in resourceOptions.ResourceDictionaryFactories)
				{
					var dictionary = factory() ?? throw new InvalidOperationException(
						"A registered resource dictionary factory returned null.");
					Resources.MergedDictionaries.Add(dictionary);
				}
			}
		}

		return Handler!.MauiContext!.Services.GetRequiredService<VisualRunnerWindow>();
	}
}
