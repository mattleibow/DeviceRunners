using System.Reflection;

namespace DeviceRunners.VisualRunners.Xunit;

class XunitWasmTestCaseInfo : ITestCaseInfo
{
	public XunitWasmTestCaseInfo(
		XunitWasmTestAssemblyInfo assembly,
		Type testClass,
		MethodInfo testMethod,
		string displayName,
		string? skipReason,
		object?[]? arguments)
	{
		TestAssembly = assembly;
		TestClass = testClass;
		TestMethod = testMethod;
		DisplayName = displayName;
		SkipReason = skipReason;
		Arguments = arguments;
	}

	public XunitWasmTestAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public Type TestClass { get; }

	public MethodInfo TestMethod { get; }

	public string DisplayName { get; }

	public string? SkipReason { get; }

	public object?[]? Arguments { get; }

	public ITestResultInfo? Result { get; private set; }

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(ITestResultInfo result)
	{
		Result = result;
		ResultReported?.Invoke(result);
	}
}
