using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

class WasmXunitTestCaseInfo : ITestCaseInfo
{
	public WasmXunitTestCaseInfo(WasmXunitAssemblyInfo assembly, ITestCase testCase)
	{
		TestAssembly = assembly;
		XunitTestCase = testCase;
	}

	public WasmXunitAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string DisplayName => XunitTestCase.DisplayName;

	public ITestCase XunitTestCase { get; }

	public ITestResultInfo? Result { get; private set; }

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(ITestResultInfo result)
	{
		Result = result;
		ResultReported?.Invoke(result);
	}
}
