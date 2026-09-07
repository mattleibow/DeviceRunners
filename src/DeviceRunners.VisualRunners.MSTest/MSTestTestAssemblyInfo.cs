using System.Reflection;

namespace DeviceRunners.VisualRunners.MSTest;

class MSTestTestAssemblyInfo : ITestAssemblyInfo
{
	readonly ITestAssemblyConfiguration _configuration = new MSTestTestAssemblyConfiguration();

	public MSTestTestAssemblyInfo(string assemblyFileName, Assembly assembly)
	{
		AssemblyFileName = assemblyFileName ?? throw new ArgumentNullException(nameof(assemblyFileName));
		Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
	}

	public MSTestTestAssemblyInfo(MSTestTestAssemblyInfo testAssembly, IReadOnlyList<MSTestTestCaseInfo> testCases)
		: this(testAssembly.AssemblyFileName, testAssembly.Assembly)
	{
		TestCases.AddRange(testCases ?? throw new ArgumentNullException(nameof(testCases)));
	}

	public string AssemblyFileName { get; }

	/// <summary>
	/// The runtime <see cref="Assembly"/> that MSTest is hosted against. Kept so the runner
	/// can pass it to <c>AddMSTest</c> without re-resolving it from the file name, which is
	/// important on platforms where <see cref="Assembly.Location"/> is empty.
	/// </summary>
	public Assembly Assembly { get; }

	ITestAssemblyConfiguration? ITestAssemblyInfo.Configuration => _configuration;

	public List<MSTestTestCaseInfo> TestCases { get; } = new();

	IReadOnlyList<ITestCaseInfo> ITestAssemblyInfo.TestCases => TestCases;
}
