namespace SampleMauiApp;

public class TestPageUITests : UITests<TestPage>
{
	[UIFact]
	public void PopulatedUsernameAndPasswordEnableLogin()
	{
		var vm = (TestViewModel)CurrentPage.BindingContext;
		vm.Username = "mattleibow";
		vm.Password = "P@55w0rd";

		var loginButton = CurrentPage.FindByName<Button>("loginButton");

		Assert.True(loginButton.IsEnabled);
	}

	[UIFact]
	public void RemovingPasswordDisablesLogin()
	{
		var vm = (TestViewModel)CurrentPage.BindingContext;
		vm.Username = "mattleibow";
		vm.Password = "P@55w0rd";

		vm.Password = "";

		var loginButton = CurrentPage.FindByName<Button>("loginButton");

		Assert.False(loginButton.IsEnabled);
	}
}
