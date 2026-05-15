using DeviceRunners.Core;

namespace DeviceRunners.VisualRunners.Blazor;

/// <summary>
/// App terminator for Blazor environments. In the browser there is no process to kill,
/// so this implementation simply logs the request.
/// </summary>
public class BlazorAppTerminator : IAppTerminator
{
	public void Terminate()
	{
		Console.WriteLine("Blazor app termination requested.");
	}
}
