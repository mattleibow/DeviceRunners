using Xunit;

namespace TestProject.Tests;

public class XunitTests : IDisposable
{
	public XunitTests()
	{
	}

	public void Dispose()
	{
	}

	[Fact]
	public void SimpleTest()
	{
		Assert.True(true);
	}

	[Fact]
	public void SimpleTest_Failed()
	{
		throw new Exception(Constants.ErrorMessage);
	}

	[Fact(Skip = Constants.SkippedReason)]
	public void SimpleTest_Skipped()
	{
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	public void DataTest(int number)
	{
		Assert.True(true);
	}
}
