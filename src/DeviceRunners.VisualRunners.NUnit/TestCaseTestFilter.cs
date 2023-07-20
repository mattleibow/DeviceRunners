using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DeviceRunners.VisualRunners.NUnit;

class TestCaseTestFilter : TestFilter
{
	readonly IList<NUnitTestCaseInfo> _testCases;

	public TestCaseTestFilter(IList<NUnitTestCaseInfo> testCases)
	{
		_testCases = testCases;
	}

	public override TNode AddToXml(TNode parentNode, bool recursive) =>
		throw new NotImplementedException();

	public override bool Match(ITest test)
	{
		foreach (var testCase in _testCases)
			if (testCase.TestCase == test)
				return true;
		return false;
	}
}
