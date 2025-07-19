using System.Runtime.CompilerServices;

namespace DeviceTestingKitApp.UITests.XunitTests;

public static partial class _Config
{
	[ModuleInitializer]
	public static void Run() => Current = "web";
}
