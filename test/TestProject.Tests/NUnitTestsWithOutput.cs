using NUnit;
using NUnit.Framework;

namespace TestProject.Tests;

public class NUnitTestsWithOutput
{
	[Test]
	public void SimpleTest_Output()
	{
		TestContext.Out.WriteLine(Constants.TestOutput);
	}

	[Test]
	public void SimpleTest_Output_Failed()
	{
		TestContext.Out.WriteLine(Constants.TestOutput);

		throw new Exception(Constants.ErrorMessage);
	}
}
