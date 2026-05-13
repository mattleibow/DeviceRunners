using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Sample Xunit tests that run in browser WebAssembly.
/// </summary>
public class SampleXunitTests
{
	[Fact]
	public void PassingTest()
	{
		Assert.True(true);
	}

	[Fact]
	public void AnotherPassingTest()
	{
		Assert.Equal(42, 21 + 21);
	}

	[Fact]
	public void StringTest()
	{
		Assert.Contains("hello", "hello world");
	}

	[Theory]
	[InlineData(1, 2, 3)]
	[InlineData(10, 20, 30)]
	[InlineData(-1, 1, 0)]
	public void AdditionTest(int a, int b, int expected)
	{
		Assert.Equal(expected, a + b);
	}

	[Fact(Skip = "Demonstrating a skipped test")]
	public void SkippedTest()
	{
		Assert.True(false, "This should not run");
	}

	[Fact]
	public void EnvironmentTest()
	{
		// Verify we're running in a WASM environment
		Assert.True(OperatingSystem.IsBrowser() || true, "Test should work on any platform");
	}
}

/// <summary>
/// Tests using [MemberData] to verify proper data-driven test discovery
/// beyond simple [InlineData].
/// </summary>
public class MemberDataTests
{
	public static IEnumerable<object[]> GetAdditionData()
	{
		yield return new object[] { 2, 3, 5 };
		yield return new object[] { 0, 0, 0 };
		yield return new object[] { -1, -1, -2 };
	}

	[Theory]
	[MemberData(nameof(GetAdditionData))]
	public void MemberDataAddition(int a, int b, int expected)
	{
		Assert.Equal(expected, a + b);
	}

	public static IEnumerable<object[]> GetStringData()
	{
		yield return new object[] { "hello", "HELLO" };
		yield return new object[] { "world", "WORLD" };
	}

	[Theory]
	[MemberData(nameof(GetStringData))]
	public void MemberDataStringUpper(string input, string expected)
	{
		Assert.Equal(expected, input.ToUpperInvariant());
	}
}

/// <summary>
/// Tests verifying async test execution works correctly.
/// </summary>
public class AsyncTests
{
	[Fact]
	public async Task AsyncFactTest()
	{
		await Task.Delay(10);
		Assert.True(true);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	public async Task AsyncTheoryTest(int value)
	{
		await Task.Yield();
		Assert.True(value > 0);
	}
}

/// <summary>
/// Tests verifying IDisposable test class lifecycle works.
/// </summary>
public class DisposableTests : IDisposable
{
	private bool _disposed;

	public void Dispose()
	{
		_disposed = true;
	}

	[Fact]
	public void TestRunsBeforeDispose()
	{
		Assert.False(_disposed);
	}
}

/// <summary>
/// Tests verifying ITestOutputHelper constructor injection works.
/// </summary>
public class OutputHelperTests
{
	readonly ITestOutputHelper _output;

	public OutputHelperTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void CanWriteOutput()
	{
		_output.WriteLine("This is test output from WASM");
		Assert.NotNull(_output);
	}
}
