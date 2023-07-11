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
		Assert.IsTrue(true);
	}

	[TestCase(1)]
	[TestCase(2)]
	[TestCase(3)]
	public void DataTest(int number)
	{
		Assert.IsTrue(true);
	}
}
