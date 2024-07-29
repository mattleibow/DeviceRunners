namespace DeviceRunners.Core;

public class DefaultAppTerminator : IAppTerminator
{
	public void Terminate()
	{
		Environment.Exit(0);
	}
}
