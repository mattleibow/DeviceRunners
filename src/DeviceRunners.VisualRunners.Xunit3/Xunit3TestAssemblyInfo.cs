using Xunit.Runner.Common;

namespace DeviceRunners.VisualRunners.Xunit3;

class Xunit3TestAssemblyInfo : ITestAssemblyInfo
{
	readonly ITestAssemblyConfiguration _configuration;

	public Xunit3TestAssemblyInfo(string assemblyFileName, TestAssemblyConfiguration? configuration = null)
	{
		AssemblyFileName = assemblyFileName ?? throw new ArgumentNullException(nameof(assemblyFileName));
		Configuration = configuration ?? new TestAssemblyConfiguration();
		_configuration = new Xunit3TestAssemblyConfiguration(Configuration);
	}

	public Xunit3TestAssemblyInfo(string assemblyFileName, TestAssemblyConfiguration configuration, IReadOnlyList<Xunit3TestCaseInfo> testCases)
	: this(assemblyFileName, configuration)
	{
		TestCases.AddRange(testCases ?? throw new ArgumentNullException(nameof(testCases)));
	}

	public Xunit3TestAssemblyInfo(Xunit3TestAssemblyInfo testAssembly, IReadOnlyList<Xunit3TestCaseInfo> testCases)
	: this(testAssembly.AssemblyFileName, testAssembly.Configuration, testCases)
	{
	}

	public string AssemblyFileName { get; }

	public TestAssemblyConfiguration Configuration { get; }

	ITestAssemblyConfiguration? ITestAssemblyInfo.Configuration => _configuration;

	public List<Xunit3TestCaseInfo> TestCases { get; } = new();

	IReadOnlyList<ITestCaseInfo> ITestAssemblyInfo.TestCases => TestCases;
}
