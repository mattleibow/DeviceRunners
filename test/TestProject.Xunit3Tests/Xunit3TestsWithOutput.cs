using Xunit;

namespace TestProject.Xunit3Tests;

public class Xunit3TestsWithOutput(ITestOutputHelper output)
{
	[Fact]
	public void SimpleTest_Output()
	{
		output.WriteLine(Constants.TestOutput);
	}

	[Fact]
	public void SimpleTest_Output_Failed()
	{
		output.WriteLine(Constants.TestOutput);

		throw new Exception(Constants.ErrorMessage);
	}
}
