namespace DeviceTestingKitApp.BlazorLibrary.XunitTests;

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

	[Fact]
	[Trait("Category", "ExpectedFailure")]
	public void FailingTest()
	{
		throw new Exception("This is meant to fail.");
	}
}
