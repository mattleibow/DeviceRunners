namespace TestProject;

public class UnitTest1
{
	[Fact]
	public void SuccessfulTest()
	{
		Assert.True(true);
	}

	[Fact(Skip = "This test is skipped.")]
	public void SkippedTest()
	{
	}

	[Fact]
	public void FailingTest()
	{
		throw new Exception("This is meant to fail.");
	}
}
