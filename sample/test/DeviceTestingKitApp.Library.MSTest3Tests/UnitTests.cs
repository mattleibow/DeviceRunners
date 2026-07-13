namespace DeviceTestingKitApp.Library.MSTest3Tests;

[TestClass]
public class UnitTests
{
	[TestMethod]
	public void SuccessfulTest()
	{
		var value = true;
		Assert.IsTrue(value);
	}

	[TestMethod]
	[Ignore("This test is skipped.")]
	public void SkippedTest()
	{
	}

	[TestMethod]
	[TestCategory("ExpectedFailure")]
	public void FailingTest()
	{
		throw new Exception("This is meant to fail.");
	}
}
