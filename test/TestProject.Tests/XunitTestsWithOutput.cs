using Xunit;
using Xunit.Abstractions;

namespace TestProject.Tests;

public class XunitTestsWithOutput
{
	readonly ITestOutputHelper _output;

	public XunitTestsWithOutput(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void SimpleTest_Output()
	{
		_output.WriteLine(Constants.TestOutput);
	}

	[Fact]
	public void SimpleTest_Output_Failed()
	{
		_output.WriteLine(Constants.TestOutput);

		throw new Exception(Constants.ErrorMessage);
	}
}
