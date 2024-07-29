namespace SampleNUnitTestProject;

public class UnitTests
{
	[Test]
	public void SuccessfulTest()
	{
		var value = true;
		Assert.That(value, Is.True);
	}

	[Test]
	[Ignore("This test is skipped.")]
	public void SkippedTest()
	{
	}

#if INCLUDE_FAILING_TESTS
	[Test]
	public void FailingTest()
	{
		throw new Exception("This is meant to fail.");
	}
#endif
}
