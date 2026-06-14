namespace DeviceRunners.VisualRunners;

public interface ITestCaseInfo
{
	ITestAssemblyInfo TestAssembly { get; }

	string DisplayName { get; }

	ITestResultInfo? Result { get; }

	event Action<ITestResultInfo>? ResultReported;

	/// <summary>
	/// The fully qualified test class name (e.g. "MyNamespace.MyClass"), when available.
	/// </summary>
	string? TestClassName { get; }

	/// <summary>
	/// The test method name (e.g. "MyMethod"), when available.
	/// </summary>
	string? TestMethodName { get; }

	/// <summary>
	/// The namespace of the test class (e.g. "MyNamespace"), when available.
	/// </summary>
	string? TestClassNamespace { get; }

	/// <summary>
	/// The traits (categories) associated with the test case, keyed by trait name.
	/// </summary>
	IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }
}
