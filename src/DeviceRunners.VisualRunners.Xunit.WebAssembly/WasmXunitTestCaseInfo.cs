namespace DeviceRunners.VisualRunners.Xunit;

class WasmXunitTestCaseInfo : ITestCaseInfo
{
	public WasmXunitTestCaseInfo(WasmXunitAssemblyInfo assembly, string displayName)
	{
		TestAssembly = assembly;
		DisplayName = displayName;
	}

	public WasmXunitAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string DisplayName { get; }

	public ITestResultInfo? Result { get; private set; }

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(ITestResultInfo result)
	{
		Result = result;
		ResultReported?.Invoke(result);
	}
}
