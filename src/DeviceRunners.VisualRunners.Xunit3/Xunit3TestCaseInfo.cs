namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3TestCaseInfo : ITestCaseInfo
{
	public Xunit3TestCaseInfo(Xunit3TestAssemblyInfo assembly, string testCaseUniqueID, string displayName, string? testClassName, string? testMethodName)
	{
		TestAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
		TestCaseUniqueID = testCaseUniqueID ?? throw new ArgumentNullException(nameof(testCaseUniqueID));
		DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
		TestClassName = testClassName;
		TestMethodName = testMethodName;
	}

	public Xunit3TestAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string AssemblyFileName => TestAssembly.AssemblyFileName;

	public string DisplayName { get; }

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
