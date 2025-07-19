namespace DeviceRunners.UIAutomation.Appium;

/// <summary>
/// This type holds the configuration options for the Appium service manager.
/// </summary>
public class AppiumServiceManagerOptions
{
	public const string DefaultHostAddress = "127.0.0.1";
	public const int DefaultHostPort = 14723;
	public static readonly TimeSpan DefaultServerStartWaitDelay = TimeSpan.FromSeconds(15);

	public string HostAddress { get; set; } = DefaultHostAddress;

	public int HostPort { get; set; } = DefaultHostPort;

	public string? LogFile { get; set; }

	public TimeSpan ServerStartWaitDelay { get; set; } = DefaultServerStartWaitDelay;
}
