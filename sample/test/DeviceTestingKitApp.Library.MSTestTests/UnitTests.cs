namespace DeviceTestingKitApp.Library.MSTestTests;

[TestClass]
public class UnitTests
{
	public static IEnumerable<(int Value, string Name)> GetTestData()
	{
		yield return (1, "first");
		yield return (2, "second");
	}

	public static IEnumerable<(int Value, string Name)> TestDataProperty =>
	[
		(1, "first"),
		(2, "second")
	];

	public static IEnumerable<(int Value, string Name)> TestDataField =
	[
		(1, "first"),
		(2, "second")
	];

	// Process-wide on purpose: MSTest's [Retry] re-runs the method in the same host, and a fresh
	// class instance is created for every attempt, so instance state cannot survive between them.
	static int _retryAttempts;

	public TestContext TestContext { get; set; }

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

	[TestMethod]
	[TestCategory("Retry")]
	[Retry(3)]
	public void FlakyTestPassesOnRetry()
	{
		// Fails on the first attempt and passes on the retry. Microsoft.Testing.Platform reports
		// every [Retry] attempt under the same test node UID, tagging the earlier one
		// retry.is-superseded; the visual runner and result channels must record only the final
		// passing attempt, not the superseded failure.
		var attempt = Interlocked.Increment(ref _retryAttempts);
		if (attempt == 1)
			Assert.Fail("Simulated flaky failure on the first attempt; the retry is expected to pass.");
	}

	[TestMethod]
	[DataRow(1)]
	public void DataRowTest(int value)
	{
		Assert.AreEqual(1, value);
	}

	[TestMethod]
	[DataRow(1, 2, 3, 4)]
	[DataRow(10, 20)]
	public void TestWithParams(params int[] values)
	{
		Assert.IsTrue(values.Length > 0);
	}

	[TestMethod]
	[DataRow(42)]
	[DataRow("alpha")]
	public void Value_RoundTrips<T>(T value)
	{
#pragma warning disable MSTEST0032 // Assertion condition is always true
		Assert.AreEqual(value, value);
#pragma warning restore MSTEST0032 // Assertion condition is always true
	}

	[TestMethod]
	[DynamicData(nameof(GetTestData))]
	public void TestWithMethod(int value, string name)
	{
		Assert.IsTrue(value > 0);
	}

	[TestMethod]
	[DynamicData(nameof(TestDataProperty))]
	public void TestWithProperty(int value, string name)
	{
		Assert.IsTrue(value > 0);
	}

	[TestMethod]
	[DynamicData(nameof(TestDataField))]
	public void TestWithField(int value, string name)
	{
		Assert.IsTrue(value > 0);
	}

	[TestMethod]
	public async Task CooperativeCancellationTest()
	{
		Assert.IsNotNull(TestContext);
		await Task.Delay(1, TestContext.CancellationToken);
	}
}
