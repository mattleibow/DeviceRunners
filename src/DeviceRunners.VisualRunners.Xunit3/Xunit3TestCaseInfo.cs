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

	public Xunit3TestResultInfo? Result { get; private set; }

	ITestResultInfo? ITestCaseInfo.Result => Result;

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(Xunit3TestResultInfo result)
	{
		Result = result;

		ResultReported?.Invoke(result);
	}
}
