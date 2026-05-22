using DeviceTestingKitApp.Formatters;

namespace DeviceTestingKitApp.BlazorLibrary.NUnitTests.Formatters;

public class CounterValueFormatterTests
{
	[Test]
	public void Format_Zero_ReturnsClickMe()
	{
		Assert.That(CounterValueFormatter.Format(0), Is.EqualTo("Click me!"));
	}

	[Test]
	public void Format_One_ReturnsSingular()
	{
		Assert.That(CounterValueFormatter.Format(1), Is.EqualTo("Clicked 1 time"));
	}

	[Test]
	[TestCase(2, "Clicked 2 times")]
	[TestCase(5, "Clicked 5 times")]
	[TestCase(100, "Clicked 100 times")]
	public void Format_Multiple_ReturnsPlural(int count, string expected)
	{
		Assert.That(CounterValueFormatter.Format(count), Is.EqualTo(expected));
	}

	[Test]
	[TestCase(-1, "Clicked -1 times")]
	[TestCase(2147483647, "Clicked 2147483647 times")]
	public void Format_EdgeCases_ReturnsPlural(int count, string expected)
	{
		Assert.That(CounterValueFormatter.Format(count), Is.EqualTo(expected));
	}
}
