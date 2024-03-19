using System.Runtime.CompilerServices;

namespace DeviceTestingKitApp.UITests;

public static partial class _Config
{
	[ModuleInitializer]
	public static void Run() => Current = "windows_msix";
}
