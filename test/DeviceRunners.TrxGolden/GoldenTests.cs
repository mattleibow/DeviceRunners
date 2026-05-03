using Xunit;

namespace DeviceRunners.TrxGolden;

/// <summary>
/// Fixed test set used to generate the golden TRX file.
/// Tests MUST NOT be changed — they define what the golden file represents.
///   PassTest           → Passed  (Fact)
///   FailTest           → Failed  (Fact, known message)
///   SkipTest           → NotExecuted / Skipped  (Fact)
///   Theory_Int         → Passed/Failed  (Theory, int params)
///   Theory_String      → Passed  (Theory, string param containing a dot)
/// </summary>
public class GoldenTests
{
	[Fact]
	public void PassTest()
	{
		Assert.Equal(1, 1);
	}

	[Fact]
	public void FailTest()
	{
		Assert.Equal(1, 2);
	}

	[Fact(Skip = "Intentionally skipped for TRX golden fixture")]
	public void SkipTest()
	{
		Assert.Fail("should not run");
	}

	[Theory]
	[InlineData(2, 2)]
	[InlineData(3, 3)]
	public void Theory_Int(int a, int b)
	{
		Assert.Equal(a, b);
	}

	/// <summary>
	/// The parameter contains a dot, which verifies className/name splitting
	/// is not confused by dots inside parameter values.
	/// </summary>
	[Theory]
	[InlineData("hello.world")]
	[InlineData("foo.bar.baz")]
	public void Theory_StringWithDots(string value)
	{
		Assert.Contains(".", value);
	}
}
