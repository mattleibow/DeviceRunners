using NUnit.Framework;

namespace DeviceRunners.TrxGolden.NUnit;

/// <summary>
/// Fixed test set used to generate the NUnit golden TRX file.
/// Tests MUST NOT be changed — they define what the golden file represents.
///   PassTest                      → Passed
///   FailTest                      → Failed  (known message)
///   IgnoreTest                    → NotExecuted / Ignored
///   TestCase_Int(1,1)             → Passed   (int params)
///   TestCase_Int(2,3)             → Failed   (int params)
///   TestCase_StringWithDots("a.b")→ Passed   (dotted string param)
/// </summary>
[TestFixture]
public class GoldenNUnitTests
{
    [Test]
    public void PassTest()
    {
        Assert.That(1, Is.EqualTo(1));
    }

    [Test]
    public void FailTest()
    {
        Assert.That(1, Is.EqualTo(2));
    }

    [Test]
    [Ignore("Intentionally ignored for TRX golden fixture")]
    public void IgnoreTest()
    {
        Assert.Fail("should not run");
    }

    [TestCase(1, 1)]
    [TestCase(2, 3)]
    public void TestCase_Int(int a, int b)
    {
        Assert.That(a, Is.EqualTo(b));
    }

    /// <summary>
    /// The parameter contains a dot, which verifies className/name splitting
    /// is not confused by dots inside parameter values.
    /// </summary>
    [TestCase("hello.world")]
    [TestCase("foo.bar.baz")]
    public void TestCase_StringWithDots(string value)
    {
        Assert.That(value, Does.Contain("."));
    }
}
