using Xunit.Sdk;

namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3TestCaseInfo : ITestCaseInfo
{
	public Xunit3TestCaseInfo(Xunit3TestAssemblyInfo assembly, ITestCase testCase)
	{
		TestAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
		TestCaseUniqueID = testCase.UniqueID;
		DisplayName = testCase.TestCaseDisplayName;
		TestClassName = testCase.TestClassName;
		TestMethodName = testCase.TestMethodName;
		TestClassNamespace = GetNamespace(TestClassName);
		Traits = ConvertTraits(testCase.Traits);
	}

	public Xunit3TestAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string AssemblyFileName => TestAssembly.AssemblyFileName;

	public string DisplayName { get; }

	/// <summary>
	/// The xunit v3 <see cref="ITestCase"/> instance from discovery.
	/// Cached so the runner can pass it directly to the executor
	/// without re-discovering every time tests are run.
	/// </summary>
	public ITestCase TestCase { get; }

	/// <summary>
	/// The unique ID for this test case, used for filtering during selective execution.
	/// </summary>
	public string TestCaseUniqueID { get; }

	/// <summary>
	/// The fully qualified test class name (e.g. "MyNamespace.MyClass").
	/// </summary>
	public string? TestClassName { get; }

	/// <summary>
	/// The test method name (e.g. "MyMethod").
	/// </summary>
	public string? TestMethodName { get; }

	/// <summary>
	/// The namespace of the test class (e.g. "MyNamespace").
	/// </summary>
	public string? TestClassNamespace { get; }

	/// <summary>
	/// The traits associated with this test case.
	/// </summary>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }

	public Xunit3TestResultInfo? Result { get; private set; }

	ITestResultInfo? ITestCaseInfo.Result => Result;

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(Xunit3TestResultInfo result)
	{
		Result = result;

		ResultReported?.Invoke(result);
	}

	static string? GetNamespace(string? className)
	{
		if (string.IsNullOrEmpty(className))
			return null;

		var index = className!.LastIndexOf('.');
		return index > 0 ? className.Substring(0, index) : null;
	}

	static IReadOnlyDictionary<string, IReadOnlyList<string>> ConvertTraits(IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits)
	{
		if (traits is null || traits.Count == 0)
			return new Dictionary<string, IReadOnlyList<string>>();

		var result = new Dictionary<string, IReadOnlyList<string>>(traits.Count);
		foreach (var pair in traits)
			result[pair.Key] = pair.Value as IReadOnlyList<string> ?? pair.Value.ToList();

		return result;
	}
}
