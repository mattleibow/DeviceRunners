using Xunit.Abstractions;

namespace Microsoft.Maui.TestUtils.DeviceTests.Sample;

public class UnitTestsWithOutput
{
	readonly ITestOutputHelper _output;

	public UnitTestsWithOutput(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void OutputTest()
	{
		_output.WriteLine("This is test output.");
	}

	[Fact]
	public void FailingOutputTest()
	{
		_output.WriteLine("This is test output.");
		throw new Exception("This is meant to fail.");
	}
}
