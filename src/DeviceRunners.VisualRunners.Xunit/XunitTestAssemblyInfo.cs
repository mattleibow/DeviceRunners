using Xunit;

namespace DeviceRunners.VisualRunners.Xunit;

class XunitTestAssemblyInfo : ITestAssemblyInfo
{
	readonly ITestAssemblyConfiguration _configuration;

	public XunitTestAssemblyInfo(string assemblyFileName, TestAssemblyConfiguration configuration)
	{
		AssemblyFileName = assemblyFileName ?? throw new ArgumentNullException(nameof(assemblyFileName));
		Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

		_configuration = new XunitTestAssemblyConfiguration(Configuration);
	}

	public XunitTestAssemblyInfo(string assemblyFileName, TestAssemblyConfiguration configuration, IReadOnlyList<XunitTestCaseInfo> testCases)
		: this(assemblyFileName, configuration)
	{
		TestCases.AddRange(testCases ?? throw new ArgumentNullException(nameof(testCases)));
	}

	public XunitTestAssemblyInfo(XunitTestAssemblyInfo testAssembly, IReadOnlyList<XunitTestCaseInfo> testCases)
		: this(testAssembly.AssemblyFileName, testAssembly.Configuration, testCases)
	{
	}

	public string AssemblyFileName { get; }

	public TestAssemblyConfiguration Configuration { get; }

	ITestAssemblyConfiguration? ITestAssemblyInfo.Configuration => _configuration;

	public List<XunitTestCaseInfo> TestCases { get; } = new();

	IReadOnlyList<ITestCaseInfo> ITestAssemblyInfo.TestCases => TestCases;
}
