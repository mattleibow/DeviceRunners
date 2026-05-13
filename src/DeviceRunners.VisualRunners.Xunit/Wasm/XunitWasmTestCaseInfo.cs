using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

class XunitWasmTestCaseInfo : ITestCaseInfo
{
	public XunitWasmTestCaseInfo(XunitWasmTestAssemblyInfo assembly, ITestCase testCase)
	{
		TestAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
	}

	public XunitWasmTestAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public ITestCase TestCase { get; }

	public string DisplayName => TestCase.DisplayName;

	public XunitWasmTestResultInfo? Result { get; private set; }

	ITestResultInfo? ITestCaseInfo.Result => Result;

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(XunitWasmTestResultInfo result)
	{
		Result = result;
		ResultReported?.Invoke(result);
	}
}
