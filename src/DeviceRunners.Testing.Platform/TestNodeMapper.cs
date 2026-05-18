using System.Globalization;

using Microsoft.Testing.Platform.Extensions.Messages;

using DeviceRunners.VisualRunners;

namespace DeviceRunners.Testing.Platform;

/// <summary>
/// Static mapping helper: TestNodeUpdateMessage → TestResultEvent and vice versa.
/// Reusable by both device-side consumers and host-side framework.
/// </summary>
public static class TestNodeMapper
{
	/// <summary>
	/// Maps a MTP TestNodeUpdateMessage to an NDJSON-serializable TestResultEvent.
	/// </summary>
	public static TestResultEvent ToTestResultEvent(TestNodeUpdateMessage message)
	{
		var node = message.TestNode;
		var uid = node.Uid.Value;
		var parentUid = message.ParentTestNodeUid?.Value;
		var displayName = node.DisplayName;

		var stateProperty = node.Properties.SingleOrDefault<TestNodeStateProperty>();

		var (type, status, errorMessage, errorStackTrace, skipReason) = stateProperty switch
		{
			PassedTestNodeStateProperty => (TestResultEvent.TypeResult, "Passed", (string?)null, (string?)null, (string?)null),
			FailedTestNodeStateProperty failed => (TestResultEvent.TypeResult, "Failed", failed.Explanation ?? string.Empty, failed.Exception?.StackTrace, (string?)null),
			ErrorTestNodeStateProperty error => (TestResultEvent.TypeResult, "Failed", error.Explanation ?? string.Empty, error.Exception?.StackTrace, (string?)null),
			TimeoutTestNodeStateProperty timeout => (TestResultEvent.TypeResult, "Failed", timeout.Explanation ?? "Test timed out", timeout.Exception?.StackTrace, (string?)null),
			SkippedTestNodeStateProperty => (TestResultEvent.TypeResult, "Skipped", (string?)null, (string?)null, (string?)null),
			InProgressTestNodeStateProperty => (TestResultEvent.TypeInProgress, (string?)null, (string?)null, (string?)null, (string?)null),
			DiscoveredTestNodeStateProperty => (TestResultEvent.TypeDiscovered, (string?)null, (string?)null, (string?)null, (string?)null),
			_ => (TestResultEvent.TypeResult, "Passed", (string?)null, (string?)null, (string?)null),
		};

		string? duration = null;
		var timingProperty = node.Properties.SingleOrDefault<TimingProperty>();
		if (timingProperty is not null)
		{
			duration = timingProperty.GlobalTiming.Duration.ToString("c", CultureInfo.InvariantCulture);
		}

		return new TestResultEvent
		{
			Type = type,
			Uid = uid,
			ParentUid = parentUid,
			DisplayName = displayName,
			Status = status,
			Duration = duration,
			ErrorMessage = errorMessage,
			ErrorStackTrace = errorStackTrace,
			SkipReason = skipReason,
			Timestamp = DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture),
		};
	}
}
