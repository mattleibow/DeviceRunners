namespace DeviceTestingKitApp.Library.NUnitTests;

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

	[Test]
	[Category("ExpectedFailure")]
	public void FailingTest()
	{
		throw new Exception("This is meant to fail.");
	}
}
