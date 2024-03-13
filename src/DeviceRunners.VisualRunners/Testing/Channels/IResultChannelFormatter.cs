namespace DeviceRunners.VisualRunners;

public interface IResultChannelFormatter
{
	void BeginTestRun(TextWriter writer, string? message = null);

	void RecordResult(ITestResultInfo result);

	void EndTestRun();
}
