using NUnit;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Builders;

namespace CommunityToolkit.DeviceRunners.VisualRunners.NUnit;

class NUnitTestListener : ITestListener
{
	readonly IReadOnlyDictionary<ITest, NUnitTestCaseInfo> _testCases;

	public NUnitTestListener(IReadOnlyDictionary<ITest, NUnitTestCaseInfo> testCases)//, Action<string> logger, string assemblyDisplayName, bool showDiagnostics)
	{
		_testCases = testCases ?? throw new ArgumentNullException(nameof(testCases));

		// if (showDiagnostics && logger != null)
		// {
		// 	DiagnosticMessageEvent += args => logger($"{assemblyDisplayName}: {args.Message.Message}");
		// }
	}

	public void SendMessage(TestMessage message)
	{
	}

	public void TestFinished(ITestResult result)
	{
		if (!_testCases.TryGetValue(result.Test, out NUnitTestCaseInfo? testCase))
		{
			// no matching reference, search by ID as a fallback
			testCase = _testCases.FirstOrDefault(kvp => kvp.Key.Id?.Equals(result.Test.Id, StringComparison.Ordinal) ?? false).Value;

			// no tests found, so we don't know what to do
			if (testCase == null)
				return;
		}

		var info = new NUnitTestResultInfo(testCase, result);
		testCase.ReportResult(info);
	}

	public void TestOutput(TestOutput output)
	{
	}

	public void TestStarted(ITest test)
	{
	}
}
