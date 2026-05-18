namespace DeviceTestingKitApp.MtpDeviceTests.Tests;

/// <summary>
/// Tests that verify device-specific behaviors.
/// These run on the actual device via the MTP runner.
/// </summary>
public class DeviceTests
{
	[Fact]
	public void DeviceHasMainThread()
	{
		// On device, we should always have a main thread
		Assert.NotNull(Thread.CurrentThread);
	}

	[Fact]
	public void CanAccessFileSystem()
	{
		var tempPath = Path.GetTempPath();
		Assert.False(string.IsNullOrEmpty(tempPath));
		Assert.True(Directory.Exists(tempPath));
	}

	[Fact]
	public async Task AsyncTestWorks()
	{
		await Task.Delay(100);
		Assert.True(true);
	}
}
