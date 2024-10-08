using NUnit;
using NUnit.Framework;

namespace TestProject.Tests;

public class NUnitTests
{
	[SetUp]
	public void SetUp()
	{
	}

	[TearDown]
	public void TearDown()
	{
	}

	[Test]
	public void SimpleTest()
	{
		var val = true;
		Assert.That(val, Is.True);
	}

	[Test]
	public void SimpleTest_Failed()
	{
		throw new Exception(Constants.ErrorMessage);
	}

	[Test]
	[Ignore(Constants.SkippedReason)]
	public void SimpleTest_Skipped()
	{
	}

	[TestCase(1)]
	[TestCase(2)]
	[TestCase(3)]
	public void DataTest(int number)
	{
		var val = true;
		Assert.That(val, Is.True);
	}
}
