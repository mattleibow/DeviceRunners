using System;

namespace DeviceTestingKitApp.BlazorLibrary.XunitTests;

public class BlazorUnitTests
{
    [Fact]
    public void TestSum()
    {
        var result = Add(1, 2);
        Assert.Equal(3, result);
    }

    [Fact]
    public void TestDifference()
    {
        var result = Subtract(5, 3);
        Assert.Equal(2, result);
    }

    [Fact]
    public void TestProduct()
    {
        var result = Multiply(3, 4);
        Assert.Equal(12, result);
    }

    [Fact]
    public void TestQuotient()
    {
        var result = Divide(10, 2);
        Assert.Equal(5, result);
    }

    [Fact]
    public void TestDivideByZero()
    {
        Assert.Throws<DivideByZeroException>(() => Divide(10, 0));
    }

#if INCLUDE_FAILING_TESTS
    [Fact]
    public void TestDemonstrateFailing()
    {
        Assert.True(false, "This test is designed to fail for demonstration purposes.");
    }
#endif

    [Fact(Skip = "This test is skipped for demonstration purposes.")]
    public void TestDemonstrateSkipped()
    {
        Assert.True(true);
    }

    // Simple calculator methods for testing
    private static int Add(int a, int b) => a + b;
    private static int Subtract(int a, int b) => a - b;
    private static int Multiply(int a, int b) => a * b;
    private static int Divide(int a, int b) => b == 0 ? throw new DivideByZeroException() : a / b;
}