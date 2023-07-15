using Xunit.Abstractions;

namespace SampleMauiApp;

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

#if INCLUDE_FAILING_TESTS
	[Fact]
	public void FailingOutputTest()
	{
		_output.WriteLine("This is test output.");
		throw new Exception("This is meant to fail.");
	}
#endif
}
