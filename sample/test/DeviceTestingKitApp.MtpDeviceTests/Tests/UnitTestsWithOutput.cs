namespace DeviceTestingKitApp.MtpDeviceTests;

public class UnitTestsWithOutput(ITestOutputHelper output)
{
	[Fact]
	public void OutputTest()
	{
		output.WriteLine("This is test output.");
	}

#if INCLUDE_FAILING_TESTS
	[Fact]
	public void FailingOutputTest()
	{
		output.WriteLine("This is test output.");
		throw new Exception("This is meant to fail.");
	}
#endif
}
