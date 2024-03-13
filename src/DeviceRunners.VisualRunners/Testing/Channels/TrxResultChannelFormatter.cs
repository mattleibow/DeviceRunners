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

		testCount = testFailed = testSucceeded = 0;

		doc = new XmlDocument();

		rootNode = doc.CreateElement("TestRun", xmlNamespace);
		rootNode.SetAttribute("id", Guid.NewGuid().ToString());
		rootNode.SetAttribute("name", TestRunName);
		rootNode.SetAttribute("runUser", TestRunUser);
		doc.AppendChild(rootNode);

		header = doc.CreateElement("Times", xmlNamespace);
		header.SetAttribute("finish", DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
		header.SetAttribute("start", DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
		header.SetAttribute("creation", DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
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
		var idx = result.TestCase.DisplayName.LastIndexOf('.');
		var testName = result.TestCase.DisplayName;  // result.TestCase.TestCase.TestMethod.Method.Name;
		var className = result.TestCase.DisplayName;  //result.TestCase.TestCase.TestMethod.TestClass.Class.Name;
		resultNode.SetAttribute("testName", testName);
		resultNode.SetAttribute("testId", id);
		resultNode.SetAttribute("duration", result.Duration.ToString("c", CultureInfo.InvariantCulture));
		resultNode.SetAttribute("computerName", "");

		if (result.Status == TestResultStatus.Failed)
		{
			testFailed++;
			var output = doc.CreateElement("Output", xmlNamespace);
			var errorInfo = doc.CreateElement("ErrorInfo", xmlNamespace);
			var message = doc.CreateElement("Message", xmlNamespace);
			message.InnerText = result.ErrorMessage;
			var stackTrace = doc.CreateElement("StackTrace", xmlNamespace);
			stackTrace.InnerText = result.ErrorStackTrace;
			output.AppendChild(errorInfo);
			errorInfo.AppendChild(message);
			errorInfo.AppendChild(stackTrace);
			resultNode.AppendChild(output);
		}
		else
		{
			testSucceeded++;
		}
		testCount++;

		resultsNode.AppendChild(resultNode);

		var testNode = doc.CreateElement("UnitTest", xmlNamespace);
		testNode.SetAttribute("name", testName);
		testNode.SetAttribute("id", id);
		testNode.SetAttribute("storage", result.TestCase.TestAssembly.AssemblyFileName);

		XmlElement properties = null;
		List<string> categories = null;

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
		var testMethodName = doc.CreateElement("TestMethod", xmlNamespace);
		testMethodName.SetAttribute("name", testName);
		testMethodName.SetAttribute("className", className);
		testMethodName.SetAttribute("codeBase", result.TestCase.TestAssembly.AssemblyFileName);
		testNode.AppendChild(testMethodName);

		testDefinitions.AppendChild(testNode);

		var testEntry = doc.CreateElement("TestEntry", xmlNamespace);
		testEntry.SetAttribute("testListId", testListId);
		testEntry.SetAttribute("testId", id);
		testEntry.SetAttribute("executionId", executionId);
		testEntries.AppendChild(testEntry);
	}

	public void EndTestRun()
	{
		header.SetAttribute("finish", DateTime.Now.ToString("O"));

		var resultSummary = doc.CreateElement("ResultSummary", xmlNamespace);
		resultSummary.SetAttribute("outcome", "Completed");

		var counters = doc.CreateElement("Counters", xmlNamespace);
		counters.SetAttribute("passed", testSucceeded.ToString(CultureInfo.InvariantCulture));
		counters.SetAttribute("failed", testFailed.ToString(CultureInfo.InvariantCulture));
		counters.SetAttribute("total", testCount.ToString(CultureInfo.InvariantCulture));

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
