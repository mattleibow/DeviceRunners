namespace DeviceTestingKitApp.DeviceTests;

public class DisposableTests : IDisposable
{
private bool _disposed;

public void Dispose()
{
_disposed = true;
}

[Fact]
public void TestRunsBeforeDispose()
{
Assert.False(_disposed);
}
}
