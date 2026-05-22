namespace DeviceTestingKitApp.MtpDeviceTests;

/// <summary>
/// Tests that verify device-specific behaviors.
/// </summary>
public class DeviceTests
{
	[Fact]
	public void DeviceHasMainThread()
	{
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
