using NUnit.Framework.Interfaces;

namespace DeviceRunners.VisualRunners.NUnit;

class WasmNUnitTestCaseInfo : ITestCaseInfo
{
	public WasmNUnitTestCaseInfo(WasmNUnitAssemblyInfo assembly, ITest testCase)
	{
		TestAssembly = assembly;
		NUnitTest = testCase;
	}

	public WasmNUnitAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string DisplayName => NUnitTest.FullName;

	public ITest NUnitTest { get; }

	public ITestResultInfo? Result { get; private set; }

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(ITestResultInfo result)
	{
		Result = result;
		ResultReported?.Invoke(result);
	}
}
