namespace DeviceRunners.VisualRunners.Maui;

/// <summary>
/// Holds resource dictionary factories registered via the configuration builder.
/// These are merged into Application.Resources at startup.
/// </summary>
internal class VisualRunnerResourceOptions
{
	internal List<Func<ResourceDictionary>> ResourceDictionaryFactories { get; } = new();
}
