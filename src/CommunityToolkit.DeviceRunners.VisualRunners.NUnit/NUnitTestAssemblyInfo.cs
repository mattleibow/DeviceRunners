using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

class NUnitTestAssemblyInfo : ITestAssemblyInfo
{
	readonly ITestAssemblyConfiguration _configuration;

	public NUnitTestAssemblyInfo(string assemblyFileName, TestAssembly testAssembly, IDictionary<string, object> configuration)
	{
		AssemblyFileName = assemblyFileName ?? throw new ArgumentNullException(nameof(assemblyFileName));
		TestAssembly = testAssembly ?? throw new ArgumentNullException(nameof(assemblyFileName));;
		Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		_configuration = new NUnitTestAssemblyConfiguration(Configuration);
	}

	public NUnitTestAssemblyInfo(string assemblyFileName, TestAssembly testAssembly, IDictionary<string, object> configuration, IReadOnlyList<NUnitTestCaseInfo> testCases)
		: this(assemblyFileName, testAssembly, configuration)
	{
		TestCases.AddRange(testCases ?? throw new ArgumentNullException(nameof(testCases)));
	}

	public NUnitTestAssemblyInfo(NUnitTestAssemblyInfo testAssembly, IReadOnlyList<NUnitTestCaseInfo> testCases)
		: this(testAssembly.AssemblyFileName, testAssembly.TestAssembly, testAssembly.Configuration, testCases)
	{
	}

	public string AssemblyFileName { get; }

	public TestAssembly TestAssembly { get; }

	public IDictionary<string, object> Configuration { get; }

	ITestAssemblyConfiguration? ITestAssemblyInfo.Configuration => _configuration;

	public List<NUnitTestCaseInfo> TestCases { get; } = new();

	IReadOnlyList<ITestCaseInfo> ITestAssemblyInfo.TestCases => TestCases;
}
