using System.Globalization;
using System.Xml;

namespace DeviceRunners.VisualRunners;

public class TrxResultChannelFormatter : IResultChannelFormatter
{
	const string XmlNs = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
	const string TestListId = "8c84fa94-04c1-424b-9868-57a2d4851a1d";
	const string AllLoadedTestListId = "19431567-8539-422a-85d7-44ee4e166bda";
	const string TestTypeId = "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b";

	TextWriter? _writer;
	DateTimeOffset _runStartTime;
	readonly List<ResultEntry> _results = [];
	int _passed, _failed, _skipped;

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
		_runStartTime = DateTimeOffset.Now;
		_results.Clear();
		_passed = _failed = _skipped = 0;
	}

	public void RecordResult(ITestResultInfo result)
	{
		var id = Guid.NewGuid().ToString();
		var executionId = Guid.NewGuid().ToString();

		var endTime = DateTimeOffset.Now;
		var startTime = endTime - result.Duration;

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

		_results.Add(new ResultEntry(
			id, executionId, displayName, testName, className,
			result.TestCase.TestAssembly.AssemblyFileName,
			result.Status, result.Duration, startTime, endTime,
			result.Output, result.ErrorMessage, result.ErrorStackTrace, result.SkipReason));

		switch (result.Status)
		{
			case TestResultStatus.Passed: _passed++; break;
			case TestResultStatus.Failed: _failed++; break;
			case TestResultStatus.Skipped: _skipped++; break;
		}
	}

	public void EndTestRun()
	{
		if (_writer is null)
			return;

		var finishTime = DateTimeOffset.Now;
		var startIso = _runStartTime.ToString("O", CultureInfo.InvariantCulture);
		var finishIso = finishTime.ToString("O", CultureInfo.InvariantCulture);

		using var xml = XmlWriter.Create(_writer, new XmlWriterSettings { Indent = true });

		xml.WriteStartElement("TestRun", XmlNs);
		xml.WriteAttributeString("id", Guid.NewGuid().ToString());
		xml.WriteAttributeString("name", TestRunName);
		xml.WriteAttributeString("runUser", TestRunUser);

		// Times
		xml.WriteStartElement("Times", XmlNs);
		xml.WriteAttributeString("creation", startIso);
		xml.WriteAttributeString("queuing", startIso);
		xml.WriteAttributeString("start", startIso);
		xml.WriteAttributeString("finish", finishIso);
		xml.WriteEndElement();

		// Results
		xml.WriteStartElement("Results", XmlNs);
		foreach (var r in _results)
			WriteUnitTestResult(xml, r);
		xml.WriteEndElement();

		// TestDefinitions
		xml.WriteStartElement("TestDefinitions", XmlNs);
		foreach (var r in _results)
			WriteUnitTest(xml, r);
		xml.WriteEndElement();

		// TestEntries
		xml.WriteStartElement("TestEntries", XmlNs);
		foreach (var r in _results)
		{
			xml.WriteStartElement("TestEntry", XmlNs);
			xml.WriteAttributeString("testListId", TestListId);
			xml.WriteAttributeString("testId", r.Id);
			xml.WriteAttributeString("executionId", r.ExecutionId);
			xml.WriteEndElement();
		}
		xml.WriteEndElement();

		// TestLists
		xml.WriteStartElement("TestLists", XmlNs);
		xml.WriteStartElement("TestList", XmlNs);
		xml.WriteAttributeString("name", "Results Not in a List");
		xml.WriteAttributeString("id", TestListId);
		xml.WriteEndElement();
		xml.WriteStartElement("TestList", XmlNs);
		xml.WriteAttributeString("name", "All Loaded Results");
		xml.WriteAttributeString("id", AllLoadedTestListId);
		xml.WriteEndElement();
		xml.WriteEndElement();

		// ResultSummary
		var total = _results.Count;
		var executed = _passed + _failed;

		xml.WriteStartElement("ResultSummary", XmlNs);
		xml.WriteAttributeString("outcome", _failed > 0 ? "Failed" : "Passed");

		xml.WriteStartElement("Counters", XmlNs);
		xml.WriteAttributeString("total", total.ToString(CultureInfo.InvariantCulture));
		xml.WriteAttributeString("executed", executed.ToString(CultureInfo.InvariantCulture));
		xml.WriteAttributeString("passed", _passed.ToString(CultureInfo.InvariantCulture));
		xml.WriteAttributeString("failed", _failed.ToString(CultureInfo.InvariantCulture));
		xml.WriteAttributeString("error", "0");
		xml.WriteAttributeString("timeout", "0");
		xml.WriteAttributeString("aborted", "0");
		xml.WriteAttributeString("inconclusive", "0");
		xml.WriteAttributeString("passedButRunAborted", "0");
		xml.WriteAttributeString("notRunnable", "0");
		// Explicitly skipped tests are NOT "notExecuted" in the TRX schema — that counter
		// means tests that could not be run due to infrastructure issues. Both the xUnit
		// and NUnit TRX adapters confirm: notExecuted="0" even when Skip/Ignore tests exist.
		xml.WriteAttributeString("notExecuted", "0");
		xml.WriteAttributeString("disconnected", "0");
		xml.WriteAttributeString("warning", "0");
		xml.WriteAttributeString("completed", "0");
		xml.WriteAttributeString("inProgress", "0");
		xml.WriteAttributeString("pending", "0");
		xml.WriteEndElement(); // Counters

		xml.WriteEndElement(); // ResultSummary
		xml.WriteEndElement(); // TestRun
	}

	static void WriteUnitTestResult(XmlWriter xml, ResultEntry r)
	{
		xml.WriteStartElement("UnitTestResult", XmlNs);
		xml.WriteAttributeString("outcome", ToTrxStatus(r.Status));
		xml.WriteAttributeString("testType", TestTypeId);
		xml.WriteAttributeString("testListId", TestListId);
		xml.WriteAttributeString("executionId", r.ExecutionId);
		xml.WriteAttributeString("testName", r.DisplayName);
		xml.WriteAttributeString("testId", r.Id);
		xml.WriteAttributeString("duration", r.Duration.ToString("c", CultureInfo.InvariantCulture));
		xml.WriteAttributeString("computerName", "");
		xml.WriteAttributeString("startTime", r.StartTime.ToString("O", CultureInfo.InvariantCulture));
		xml.WriteAttributeString("endTime", r.EndTime.ToString("O", CultureInfo.InvariantCulture));
		xml.WriteAttributeString("relativeResultsDirectory", r.ExecutionId);

		switch (r.Status)
		{
			case TestResultStatus.Failed:
				xml.WriteStartElement("Output", XmlNs);
				if (r.Output is not null)
					xml.WriteElementString("StdOut", XmlNs, r.Output);
				xml.WriteStartElement("ErrorInfo", XmlNs);
				xml.WriteElementString("Message", XmlNs, r.ErrorMessage ?? "");
				xml.WriteElementString("StackTrace", XmlNs, r.ErrorStackTrace ?? "");
				xml.WriteEndElement(); // ErrorInfo
				xml.WriteEndElement(); // Output
				break;

			case TestResultStatus.Passed:
				if (r.Output is not null)
				{
					xml.WriteStartElement("Output", XmlNs);
					xml.WriteElementString("StdOut", XmlNs, r.Output);
					xml.WriteEndElement();
				}
				break;

			case TestResultStatus.Skipped:
				if (r.SkipReason is not null)
				{
					xml.WriteStartElement("Output", XmlNs);
					xml.WriteElementString("StdOut", XmlNs, r.SkipReason);
					xml.WriteStartElement("ErrorInfo", XmlNs);
					xml.WriteElementString("Message", XmlNs, r.SkipReason);
					xml.WriteEndElement(); // ErrorInfo
					xml.WriteEndElement(); // Output
				}
				break;
		}

		xml.WriteEndElement(); // UnitTestResult
	}

	static void WriteUnitTest(XmlWriter xml, ResultEntry r)
	{
		xml.WriteStartElement("UnitTest", XmlNs);
		xml.WriteAttributeString("name", r.DisplayName);
		xml.WriteAttributeString("id", r.Id);
		xml.WriteAttributeString("storage", r.AssemblyFileName);

		xml.WriteStartElement("Execution", XmlNs);
		xml.WriteAttributeString("id", r.ExecutionId);
		xml.WriteEndElement();

		xml.WriteStartElement("TestMethod", XmlNs);
		xml.WriteAttributeString("name", r.TestName);
		xml.WriteAttributeString("className", r.ClassName);
		xml.WriteAttributeString("codeBase", r.AssemblyFileName);
		xml.WriteEndElement();

		xml.WriteEndElement(); // UnitTest
	}

	static string ToTrxStatus(TestResultStatus result) =>
		result switch
		{
			TestResultStatus.NotRun => "NotRunnable",
			TestResultStatus.Skipped => "NotExecuted",
			_ => result.ToString(),
		};

	sealed record ResultEntry(
		string Id, string ExecutionId, string DisplayName, string TestName, string ClassName,
		string AssemblyFileName, TestResultStatus Status, TimeSpan Duration,
		DateTimeOffset StartTime, DateTimeOffset EndTime,
		string? Output, string? ErrorMessage, string? ErrorStackTrace, string? SkipReason);
}
