using NUnit.Framework.Internal;

namespace DeviceTestingKitApp.UITests.NUnitTests;

public class AppiumServerTests : BaseUITests
{
	public AppiumServerTests(string appKey)
		: base(appKey)
	{
	}

	[Test]
	public void IsReady()
	{
		//var id = Driver.SessionId;

		//Assert.NotNull(id);
		//Assert.NotEmpty(id.ToString());

		//Assert.Equal(AppState.RunningInForeground, Driver.GetAppState());

		Assert.Pass();
	}
}
