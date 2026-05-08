using Xunit;

namespace TestProject.Xunit3Tests;

public class Xunit3TestsWithOutput : IDisposable
{
	readonly ITestOutputHelper _output;

	public Xunit3TestsWithOutput(ITestOutputHelper output)
	{
		_output = output;
	}

	public void Dispose()
	{
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
