using CommunityToolkit.DeviceRunners.Xunit.VisualRunner.Maui.Pages;

namespace CommunityToolkit.DeviceRunners.Xunit.VisualRunner.Maui;

public partial class VisualRunnerAppShell : Shell
{
	public VisualRunnerAppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("testrunner/assembly", typeof(TestAssemblyPage));
		Routing.RegisterRoute("testrunner/assembly/result", typeof(TestResultPage));
	}
}
