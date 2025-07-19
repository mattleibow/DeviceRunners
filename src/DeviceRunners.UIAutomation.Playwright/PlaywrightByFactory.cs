namespace DeviceRunners.UIAutomation.Playwright;

public class PlaywrightByFactory : IPlaywrightByFactory
{
	public virtual PlaywrightBy Create(PlaywrightAutomatedApp app) => new PlaywrightBy();
}
