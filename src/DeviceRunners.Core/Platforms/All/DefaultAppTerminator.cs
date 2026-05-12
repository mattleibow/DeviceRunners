namespace DeviceRunners.Core;

#if !ANDROID && !IOS && !MACCATALYST && !WINDOWS

public class DefaultAppTerminator : IAppTerminator
{
	public void Terminate()
	{
		throw new InvalidOperationException("Unable to terminate a generic process.");
	}
}

#endif
