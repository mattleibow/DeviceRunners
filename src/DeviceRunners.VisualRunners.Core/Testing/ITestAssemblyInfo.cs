using System.Reflection;

namespace DeviceRunners.VisualRunners;

public interface ITestAssemblyInfo
{
	string AssemblyFileName { get; }

	ITestAssemblyConfiguration? Configuration { get; }

	IReadOnlyList<ITestCaseInfo> TestCases { get; }
}
