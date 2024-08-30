namespace DeviceTestingKitApp.UITests.XunitTests;

/// <summary>
/// xUnit does not support parameterized tests yet, so we have to use multiple
/// assemblies - one for each platform. We could maybe use environment variables,
/// but this is not something Visual Studio allows for in the test explorer.
/// </summary>
public static partial class _Config
{
	private static string? s_current;

	public static string Current
	{
		get => s_current ?? throw new InvalidOperationException("No UI test app was specified.");
		set => s_current = value;
	}
}
