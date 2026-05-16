using Xunit;

namespace DeviceTestingKitApp.BlazorLibrary.Xunit3Tests;

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

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	public void ParameterizedTest(int number)
	{
		Assert.NotEqual(0, number);
	}
}
