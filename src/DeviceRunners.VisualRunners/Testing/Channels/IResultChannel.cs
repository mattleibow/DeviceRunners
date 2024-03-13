namespace DeviceRunners.VisualRunners;

public interface IResultChannel
{
	bool IsOpen { get; }

	Task<bool> OpenChannel(string? message = null);

	void RecordResult(ITestResultInfo testResult);

	Task CloseChannel();
}
