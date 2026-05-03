using System.Globalization;
using System.Xml;

namespace DeviceRunners.VisualRunners;

public class TrxResultChannelFormatter : IResultChannelFormatter
{
	const string xmlNamespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
	const string testListId = "8c84fa94-04c1-424b-9868-57a2d4851a1d";

	TextWriter? _writer;

	XmlDocument doc;
	int testCount;
	int testFailed;
	int testSucceeded;
	int testSkipped;
	XmlElement rootNode;
	XmlElement resultsNode;
	XmlElement testDefinitions;
	XmlElement header;
	XmlElement testEntries;

	/// <summary>
	/// The key name of the trait that is used for writing the Category field to the report.
	/// </summary>
	public string CategoryTraitName { get; set; } = "Category";

	/// <summary>
	/// The name of the test run to include in the report header.
	/// </summary>
	public string TestRunName { get; set; } = "";

	/// <summary>
	/// The name of the user running the report header.
	/// </summary>
	public string TestRunUser { get; set; } = "";

	public void BeginTestRun(TextWriter writer, string? message = null)
	{
		_writer = writer;

		testCount = testFailed = testSucceeded = testSkipped = 0;

		doc = new XmlDocument();

		rootNode = doc.CreateElement("TestRun", xmlNamespace);
		rootNode.SetAttribute("id", Guid.NewGuid().ToString());
		rootNode.SetAttribute("name", TestRunName);
		rootNode.SetAttribute("runUser", TestRunUser);
		doc.AppendChild(rootNode);

		var now = DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture);
		header = doc.CreateElement("Times", xmlNamespace);
		header.SetAttribute("creation", now);
		header.SetAttribute("queuing", now);
		header.SetAttribute("start", now);
		header.SetAttribute("finish", now);
		rootNode.AppendChild(header);

		resultsNode = doc.CreateElement("Results", xmlNamespace);
		rootNode.AppendChild(resultsNode);

		testDefinitions = doc.CreateElement("TestDefinitions", xmlNamespace);
		rootNode.AppendChild(testDefinitions);

		testEntries = doc.CreateElement("TestEntries", xmlNamespace);
		rootNode.AppendChild(testEntries);

		var testLists = doc.CreateElement("TestLists", xmlNamespace);
		var testList = doc.CreateElement("TestList", xmlNamespace);
		testList.SetAttribute("name", "Results Not in a List");
		testList.SetAttribute("id", testListId);
		testLists.AppendChild(testList);
		var allTestList = doc.CreateElement("TestList", xmlNamespace);
		allTestList.SetAttribute("name", "All Loaded Results");
		allTestList.SetAttribute("id", "19431567-8539-422a-85d7-44ee4e166bda");
		testLists.AppendChild(allTestList);
		rootNode.AppendChild(testLists);
	}

	public void RecordResult(ITestResultInfo result)
	{
		var id = Guid.NewGuid().ToString();
		var executionId = Guid.NewGuid().ToString();

		var resultNode = doc.CreateElement("UnitTestResult", xmlNamespace);
		resultNode.SetAttribute("outcome", ToTrxStatus(result.Status));
		resultNode.SetAttribute("testType", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b");
		resultNode.SetAttribute("testListId", testListId);
		resultNode.SetAttribute("executionId", executionId);

		// Split "Namespace.ClassName.MethodName" or "Namespace.ClassName.Method(params)"
		// → TestMethod.name = method name only (no params, matching dotnet test behaviour)
		//   TestMethod.className = everything before the method name
		// We strip params first so dots inside parameter values (e.g. "Method(1.5, \"a.b\")")
		// never corrupt the class name.
		var displayName = result.TestCase.DisplayName;
		var parenIdx = displayName.IndexOf('(');
		var nameForSplit = parenIdx >= 0 ? displayName[..parenIdx] : displayName;
		var dotIdx = nameForSplit.LastIndexOf('.');
		var testName = dotIdx >= 0 ? nameForSplit[(dotIdx + 1)..] : nameForSplit;
		var className = dotIdx >= 0 ? nameForSplit[..dotIdx] : nameForSplit;

		var endTime = DateTimeOffset.Now;
		var startTime = endTime - result.Duration;

		resultNode.SetAttribute("testName", displayName);
		resultNode.SetAttribute("testId", id);
		resultNode.SetAttribute("duration", result.Duration.ToString("c", CultureInfo.InvariantCulture));
		resultNode.SetAttribute("computerName", "");
		resultNode.SetAttribute("startTime", startTime.ToString("O", CultureInfo.InvariantCulture));
		resultNode.SetAttribute("endTime", endTime.ToString("O", CultureInfo.InvariantCulture));
		resultNode.SetAttribute("relativeResultsDirectory", executionId);

		if (result.Status == TestResultStatus.Failed)
		{
			testFailed++;
			var output = doc.CreateElement("Output", xmlNamespace);
			var errorInfo = doc.CreateElement("ErrorInfo", xmlNamespace);
			var message = doc.CreateElement("Message", xmlNamespace);
			message.InnerText = result.ErrorMessage ?? string.Empty;
			var stackTrace = doc.CreateElement("StackTrace", xmlNamespace);
			stackTrace.InnerText = result.ErrorStackTrace ?? string.Empty;
			errorInfo.AppendChild(message);
			errorInfo.AppendChild(stackTrace);
			output.AppendChild(errorInfo);
			if (result.Output is not null)
			{
				var stdOut = doc.CreateElement("StdOut", xmlNamespace);
				stdOut.InnerText = result.Output;
				output.InsertBefore(stdOut, errorInfo);
			}
			resultNode.AppendChild(output);
		}
		else if (result.Status == TestResultStatus.Passed)
		{
			testSucceeded++;
			if (result.Output is not null)
			{
				var output = doc.CreateElement("Output", xmlNamespace);
				var stdOut = doc.CreateElement("StdOut", xmlNamespace);
				stdOut.InnerText = result.Output;
				output.AppendChild(stdOut);
				resultNode.AppendChild(output);
			}
		}
		else if (result.Status == TestResultStatus.Skipped)
		{
			testSkipped++;
			if (result.SkipReason is not null)
			{
				var output = doc.CreateElement("Output", xmlNamespace);
				var stdOut = doc.CreateElement("StdOut", xmlNamespace);
				stdOut.InnerText = result.SkipReason;
				output.AppendChild(stdOut);
				var errorInfo = doc.CreateElement("ErrorInfo", xmlNamespace);
				var message = doc.CreateElement("Message", xmlNamespace);
				message.InnerText = result.SkipReason;
				errorInfo.AppendChild(message);
				output.AppendChild(errorInfo);
				resultNode.AppendChild(output);
			}
		}
		testCount++;

		resultsNode.AppendChild(resultNode);

		var testNode = doc.CreateElement("UnitTest", xmlNamespace);
		testNode.SetAttribute("name", displayName);
		testNode.SetAttribute("id", id);
		testNode.SetAttribute("storage", result.TestCase.TestAssembly.AssemblyFileName);

		XmlElement? properties = null;
		List<string>? categories = null;

		/*
		foreach (var prop in result.TestCase.TestCase.Traits)
		{
			if (prop.Key == CategoryTraitName)
			{
				categories = prop.Value;
				continue;
			}
			foreach (var v in prop.Value)
			{
				if (properties == null)
				{
					properties = doc.CreateElement("Properties", xmlNamespace);
					testNode.AppendChild(properties);
				}

				var property = doc.CreateElement("Property", xmlNamespace);
				var key = doc.CreateElement("Key", xmlNamespace);
				key.InnerText = prop.Key;
				property.AppendChild(key);
				var value = doc.CreateElement("Value", xmlNamespace);
				value.InnerText = v;
				property.AppendChild(value);
				properties.AppendChild(property);
			}
		}
		*/

		if (categories != null && categories.Any())
		{
			var testCategory = doc.CreateElement("TestCategory", xmlNamespace);
			foreach (var category in categories)
			{
				var item = doc.CreateElement("TestCategoryItem", xmlNamespace);
				item.SetAttribute("TestCategory", category);
				testCategory.AppendChild(item);
			}
			testNode.AppendChild(testCategory);
		}
		var execution = doc.CreateElement("Execution", xmlNamespace);
		execution.SetAttribute("id", executionId);
		testNode.AppendChild(execution);
		var testMethodNode = doc.CreateElement("TestMethod", xmlNamespace);
		testMethodNode.SetAttribute("name", testName);
		testMethodNode.SetAttribute("className", className);
		testMethodNode.SetAttribute("codeBase", result.TestCase.TestAssembly.AssemblyFileName);
		testNode.AppendChild(testMethodNode);

		testDefinitions.AppendChild(testNode);

		var testEntry = doc.CreateElement("TestEntry", xmlNamespace);
		testEntry.SetAttribute("testListId", testListId);
		testEntry.SetAttribute("testId", id);
		testEntry.SetAttribute("executionId", executionId);
		testEntries.AppendChild(testEntry);
	}

	public void EndTestRun()
	{
		header.SetAttribute("finish", DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture));

		var resultSummary = doc.CreateElement("ResultSummary", xmlNamespace);
		resultSummary.SetAttribute("outcome", testFailed > 0 ? "Failed" : "Passed");

		var counters = doc.CreateElement("Counters", xmlNamespace);
		var executed = testSucceeded + testFailed;
		counters.SetAttribute("total", testCount.ToString(CultureInfo.InvariantCulture));
		counters.SetAttribute("executed", executed.ToString(CultureInfo.InvariantCulture));
		counters.SetAttribute("passed", testSucceeded.ToString(CultureInfo.InvariantCulture));
		counters.SetAttribute("failed", testFailed.ToString(CultureInfo.InvariantCulture));
		counters.SetAttribute("error", "0");
		counters.SetAttribute("timeout", "0");
		counters.SetAttribute("aborted", "0");
		counters.SetAttribute("inconclusive", "0");
		counters.SetAttribute("passedButRunAborted", "0");
		counters.SetAttribute("notRunnable", "0");
		// Explicitly skipped tests are NOT "notExecuted" in the TRX schema — that counter
		// means tests that could not be run due to infrastructure issues. Both the xUnit
		// and NUnit TRX adapters confirm: notExecuted="0" even when Skip/Ignore tests exist.
		counters.SetAttribute("notExecuted", "0");
		counters.SetAttribute("disconnected", "0");
		counters.SetAttribute("warning", "0");
		counters.SetAttribute("completed", "0");
		counters.SetAttribute("inProgress", "0");
		counters.SetAttribute("pending", "0");

		resultSummary.AppendChild(counters);
		rootNode.AppendChild(resultSummary);

		doc.Save(_writer);
	}

	static string ToTrxStatus(TestResultStatus result) =>
		result switch
		{
			TestResultStatus.NotRun => "NotRunnable",
			TestResultStatus.Skipped => "NotExecuted",
			_ => result.ToString(),
		};
}
