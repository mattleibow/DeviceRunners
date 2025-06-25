using DeviceRunners.SourceGenerators;
using Xunit;

namespace SimpleDeviceTest;

[DeviceTestApp(AppTitle = "Simple Device Test", AppId = "com.test.simpledevicetest")]
public class Tests
{
    [Fact]
    public void SimpleTest()
    {
        Assert.True(true);
    }

    [Fact]
    public void AnotherTest()
    {
        var value = 42;
        Assert.Equal(42, value);
    }
}