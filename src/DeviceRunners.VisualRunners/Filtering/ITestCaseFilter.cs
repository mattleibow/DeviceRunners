namespace DeviceRunners.VisualRunners;

/// <summary>
/// A parsed test-case filter that can determine whether a given test case matches.
/// </summary>
public interface ITestCaseFilter
{
	/// <summary>
	/// Determines whether the given <paramref name="testCase"/> matches the filter.
	/// </summary>
	bool Matches(ITestCaseInfo testCase);
}
