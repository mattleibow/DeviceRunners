namespace SampleXunitTestProject;

public class UnitTests
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

#if INCLUDE_FAILING_TESTS
	[Fact]
	public void FailingTest()
	{
		throw new Exception("This is meant to fail.");
	}
#endif
}
