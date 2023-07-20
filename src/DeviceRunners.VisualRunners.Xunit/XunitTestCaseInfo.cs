using Xunit.Abstractions;

namespace DeviceRunners.VisualRunners.Xunit;

class XunitTestCaseInfo : ITestCaseInfo
{
	public XunitTestCaseInfo(XunitTestAssemblyInfo assembly, ITestCase testCase)
	{
		TestAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
	}

	public XunitTestAssemblyInfo TestAssembly { get; }
	
	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string AssemblyFileName => TestAssembly.AssemblyFileName;

	public string DisplayName => TestCase.DisplayName;

	public ITestCase TestCase { get; }

	public XunitTestResultInfo? Result { get; private set; }

	ITestResultInfo? ITestCaseInfo.Result => Result;

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(XunitTestResultInfo result)
	{
		Result = result;

		ResultReported?.Invoke(result);
	}
}
