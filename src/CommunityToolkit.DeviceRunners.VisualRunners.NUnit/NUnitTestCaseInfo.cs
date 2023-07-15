using NUnit.Framework.Interfaces;

namespace CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

class NUnitTestCaseInfo : ITestCaseInfo
{
	public NUnitTestCaseInfo(NUnitTestAssemblyInfo assembly, ITest testCase)
	{
		TestAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
		TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
	}

	public NUnitTestAssemblyInfo TestAssembly { get; }

	ITestAssemblyInfo ITestCaseInfo.TestAssembly => TestAssembly;

	public string AssemblyFileName => TestAssembly.AssemblyFileName;

	public string DisplayName => TestCase.FullName;

	public ITest TestCase { get; }

	public NUnitTestResultInfo? Result { get; private set; }

	ITestResultInfo? ITestCaseInfo.Result => Result;

	public event Action<ITestResultInfo>? ResultReported;

	public void ReportResult(NUnitTestResultInfo result)
	{
		Result = result;

		ResultReported?.Invoke(result);
	}
}