namespace DeviceRunners.UIAutomation.Playwright;

public interface IPlaywrightByFactory
{
	PlaywrightBy Create(PlaywrightAutomatedApp app);
}
