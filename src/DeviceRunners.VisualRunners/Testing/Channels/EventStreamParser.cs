using System.Globalization;
using System.Text.Json;

namespace DeviceRunners.VisualRunners;

/// <summary>
/// Parses NDJSON lines produced by <see cref="EventStreamFormatter"/> back into
/// <see cref="TestResultEvent"/> objects and can reconstruct <see cref="ITestResultInfo"/>.
/// </summary>
public static class EventStreamParser
{
	/// <summary>
	/// Parses a single NDJSON line into a <see cref="TestResultEvent"/>.
	/// Returns null if the line is empty or not valid JSON.
	/// </summary>
	public static TestResultEvent? Parse(string line)
	{
		if (string.IsNullOrWhiteSpace(line))
			return null;

		try
		{
			return JsonSerializer.Deserialize<TestResultEvent>(line);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Converts a <see cref="TestResultEvent"/> of type "result" into an <see cref="ITestResultInfo"/>.
	/// </summary>
	public static ITestResultInfo ToTestResultInfo(TestResultEvent evt)
	{
		var status = evt.Status switch
		{
			"Passed" => TestResultStatus.Passed,
			"Failed" => TestResultStatus.Failed,
			"Skipped" => TestResultStatus.Skipped,
			_ => TestResultStatus.NotRun,
		};

		var duration = TimeSpan.Zero;
		if (evt.Duration is not null)
			TimeSpan.TryParseExact(evt.Duration, "c", CultureInfo.InvariantCulture, out duration);

		return new ParsedTestResultInfo(
			displayName: evt.DisplayName ?? "",
			assemblyFileName: evt.Assembly ?? "",
			status: status,
			duration: duration,
			output: evt.Output,
			errorMessage: evt.ErrorMessage,
			errorStackTrace: evt.ErrorStackTrace,
			skipReason: evt.SkipReason);
	}

	/// <summary>
	/// A concrete implementation of <see cref="ITestResultInfo"/> reconstructed from event data.
	/// </summary>
	sealed class ParsedTestResultInfo : ITestResultInfo
	{
		public ParsedTestResultInfo(
			string displayName,
			string assemblyFileName,
			TestResultStatus status,
			TimeSpan duration,
			string? output,
			string? errorMessage,
			string? errorStackTrace,
			string? skipReason)
		{
			TestCase = new ParsedTestCaseInfo(displayName, assemblyFileName);
			Status = status;
			Duration = duration;
			Output = output;
			ErrorMessage = errorMessage;
			ErrorStackTrace = errorStackTrace;
			SkipReason = skipReason;
		}

		public ITestCaseInfo TestCase { get; }
		public TestResultStatus Status { get; }
		public TimeSpan Duration { get; }
		public string? Output { get; }
		public string? ErrorMessage { get; }
		public string? ErrorStackTrace { get; }
		public string? SkipReason { get; }
	}

	sealed class ParsedTestCaseInfo : ITestCaseInfo
	{
		public ParsedTestCaseInfo(string displayName, string assemblyFileName)
		{
			DisplayName = displayName;
			TestAssembly = new ParsedTestAssemblyInfo(assemblyFileName);
		}

		public ITestAssemblyInfo TestAssembly { get; }
		public string DisplayName { get; }
		public ITestResultInfo? Result => null;
		public event Action<ITestResultInfo>? ResultReported { add { } remove { } }
	}

	sealed class ParsedTestAssemblyInfo : ITestAssemblyInfo
	{
		public ParsedTestAssemblyInfo(string assemblyFileName)
		{
			AssemblyFileName = assemblyFileName;
		}

		public string AssemblyFileName { get; }
		public ITestAssemblyConfiguration? Configuration => null;
		public IReadOnlyList<ITestCaseInfo> TestCases => [];
	}
}
