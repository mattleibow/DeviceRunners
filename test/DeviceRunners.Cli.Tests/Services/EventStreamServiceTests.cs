using DeviceRunners.Cli.Services;
using DeviceRunners.VisualRunners;

namespace DeviceRunners.Cli.Tests;

public class EventStreamServiceTests
{
    /// <summary>
    /// Creates a mock IResultChannel that records calls for assertions.
    /// </summary>
    static (EventStreamService service, MockResultChannel channel) CreateService()
    {
        var channel = new MockResultChannel();
        var service = new EventStreamService(channel);
        return (service, channel);
    }

    [Fact]
    public void ReceiveData_SingleCompleteLine_ParsesAndForwardsResult()
    {
        var (service, channel) = CreateService();
        var json = TestResultEvent.FromInfo(CreatePassedResult("Test1")).ToString();

        service.ReceiveData(json + "\n");

        Assert.Equal(1, service.TotalCount);
        Assert.Equal(1, service.PassedCount);
        Assert.Equal(0, service.FailedCount);
        Assert.Single(channel.RecordedResults);
    }

    [Fact]
    public void ReceiveData_MultipleLines_ParsesAll()
    {
        var (service, channel) = CreateService();
        var line1 = TestResultEvent.FromInfo(CreatePassedResult("Test1")).ToString();
        var line2 = TestResultEvent.FromInfo(CreateFailedResult("Test2")).ToString();

        service.ReceiveData(line1 + "\n" + line2 + "\n");

        Assert.Equal(2, service.TotalCount);
        Assert.Equal(1, service.PassedCount);
        Assert.Equal(1, service.FailedCount);
    }

    [Fact]
    public void ReceiveData_ChunkedLine_BuffersUntilNewline()
    {
        var (service, channel) = CreateService();
        var json = TestResultEvent.FromInfo(CreatePassedResult("Test1")).ToString();

        // Send in two chunks — no newline yet
        service.ReceiveData(json[..(json.Length / 2)]);
        Assert.Equal(0, service.TotalCount);

        // Complete the line
        service.ReceiveData(json[(json.Length / 2)..] + "\n");
        Assert.Equal(1, service.TotalCount);
    }

    [Fact]
    public void Flush_ProcessesRemainingBuffer()
    {
        var (service, channel) = CreateService();
        var json = TestResultEvent.FromInfo(CreatePassedResult("Test1")).ToString();

        // Send without newline
        service.ReceiveData(json);
        Assert.Equal(0, service.TotalCount);

        service.Flush();
        Assert.Equal(1, service.TotalCount);
    }

    [Fact]
    public void Flush_EmptyBuffer_DoesNothing()
    {
        var (service, _) = CreateService();

        service.Flush();

        Assert.Equal(0, service.TotalCount);
    }

    [Fact]
    public void ReceiveData_PingMessage_IsIgnored()
    {
        var (service, channel) = CreateService();

        service.ReceiveData("ping\n");

        Assert.Equal(0, service.TotalCount);
        Assert.Empty(channel.RecordedResults);
    }

    [Fact]
    public void ReceiveData_InvalidJson_RaisesUnparseableLine()
    {
        var (service, _) = CreateService();
        string? unparsedLine = null;
        service.UnparseableLine += (_, e) => unparsedLine = e.Line;

        service.ReceiveData("not valid json\n");

        Assert.Equal(0, service.TotalCount);
        Assert.Equal("not valid json", unparsedLine);
    }

    [Fact]
    public void ReceiveData_BeginEvent_RaisesTestRunStarted()
    {
        var (service, _) = CreateService();
        string? message = null;
        service.TestRunStarted += (_, e) => message = e.Message;

        var json = TestResultEvent.Begin("hello").ToString();
        service.ReceiveData(json + "\n");

        Assert.Equal("hello", message);
        Assert.Equal(0, service.TotalCount);
    }

    [Fact]
    public void ReceiveData_EndEvent_RaisesTestRunEnded()
    {
        var (service, _) = CreateService();
        var ended = false;
        service.TestRunEnded += (_, _) => ended = true;

        var json = TestResultEvent.End().ToString();
        service.ReceiveData(json + "\n");

        Assert.True(ended);
    }

    [Fact]
    public void ReceiveData_ResultEvent_RaisesTestResultRecorded()
    {
        var (service, _) = CreateService();
        TestResultRecordedEventArgs? recorded = null;
        service.TestResultRecorded += (_, e) => recorded = e;

        var json = TestResultEvent.FromInfo(CreatePassedResult("MyTest")).ToString();
        service.ReceiveData(json + "\n");

        Assert.NotNull(recorded);
        Assert.Equal("MyTest", recorded.Result.TestCase.DisplayName);
        Assert.Equal(TestResultStatus.Passed, recorded.Result.Status);
    }

    [Fact]
    public void Counters_TrackAllStatuses()
    {
        var (service, _) = CreateService();

        service.ReceiveData(TestResultEvent.FromInfo(CreatePassedResult("P1")).ToString() + "\n");
        service.ReceiveData(TestResultEvent.FromInfo(CreatePassedResult("P2")).ToString() + "\n");
        service.ReceiveData(TestResultEvent.FromInfo(CreateFailedResult("F1")).ToString() + "\n");
        service.ReceiveData(TestResultEvent.FromInfo(CreateSkippedResult("S1")).ToString() + "\n");

        Assert.Equal(4, service.TotalCount);
        Assert.Equal(2, service.PassedCount);
        Assert.Equal(1, service.FailedCount);
        Assert.Equal(1, service.SkippedCount);
    }

    [Fact]
    public void ReceiveData_CrLfLineEndings_ParsesCorrectly()
    {
        var (service, _) = CreateService();
        var json = TestResultEvent.FromInfo(CreatePassedResult("Test1")).ToString();

        service.ReceiveData(json + "\r\n");

        Assert.Equal(1, service.TotalCount);
    }

    // ── Helpers ──────────────────────────────────────────────

    static ITestResultInfo CreatePassedResult(string name) =>
        new StubTestResultInfo(name, TestResultStatus.Passed);

    static ITestResultInfo CreateFailedResult(string name) =>
        new StubTestResultInfo(name, TestResultStatus.Failed, errorMessage: "boom", errorStackTrace: "at Test");

    static ITestResultInfo CreateSkippedResult(string name) =>
        new StubTestResultInfo(name, TestResultStatus.Skipped, skipReason: "not today");

    sealed class StubTestResultInfo(
        string displayName,
        TestResultStatus status,
        string? errorMessage = null,
        string? errorStackTrace = null,
        string? skipReason = null) : ITestResultInfo
    {
        public ITestCaseInfo TestCase { get; } = new StubTestCaseInfo(displayName);
        public TestResultStatus Status { get; } = status;
        public TimeSpan Duration { get; } = TimeSpan.FromMilliseconds(42);
        public string? Output => null;
        public string? ErrorMessage { get; } = errorMessage;
        public string? ErrorStackTrace { get; } = errorStackTrace;
        public string? SkipReason { get; } = skipReason;
    }

    sealed class StubTestCaseInfo(string displayName) : ITestCaseInfo
    {
        public ITestAssemblyInfo TestAssembly { get; } = new StubTestAssemblyInfo();
        public string DisplayName { get; } = displayName;
        public ITestResultInfo? Result => null;
        public event Action<ITestResultInfo>? ResultReported { add { } remove { } }
    }

    sealed class StubTestAssemblyInfo : ITestAssemblyInfo
    {
        public string AssemblyFileName => "test.dll";
        public ITestAssemblyConfiguration? Configuration => null;
        public IReadOnlyList<ITestCaseInfo> TestCases => [];
    }

    /// <summary>
    /// Minimal IResultChannel that just records RecordResult calls.
    /// </summary>
    sealed class MockResultChannel : IResultChannel
    {
        public List<ITestResultInfo> RecordedResults { get; } = [];
        public bool IsOpen { get; private set; }

        public Task<bool> OpenChannel(string? message = null)
        {
            IsOpen = true;
            return Task.FromResult(true);
        }

        public void RecordResult(ITestResultInfo testResult)
        {
            RecordedResults.Add(testResult);
        }

        public Task CloseChannel()
        {
            IsOpen = false;
            return Task.CompletedTask;
        }
    }
}
